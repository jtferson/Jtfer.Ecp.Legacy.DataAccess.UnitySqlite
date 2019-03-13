using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jtfer.Ecp.Legacy.DataAccess.UnitySqlite
{
    public class SqliteConnection : DbConnectionBaseLegacy
    {
        protected override string DatabaseName => throw new NotImplementedException();
        SQLiteConnection _instance;

        public override IEnumerable<T> Get<T>()
        {
            return _instance.Table<T>().ToArray();
        }

        public override IEnumerable<T> Get<T>(Expression<Func<T, bool>> query)
        {
            var func = query.Compile();
            return _instance.Table<T>().ToArray().Where(func);
        }

        public override void Update<T>(IEnumerable<T> dtos)
        {
            _instance.Update(dtos);
        }

        public override bool Update<T>(T dto)
        {
            _instance.Update(dto);
            return true;
        }

        public override bool Upsert<T>(T dto)
        {
            var obj = _instance.Find<T>(dto.Id);
            if (obj != null)
            {
                _instance.Update(dto);
                return false;
            }
            else
            {
                _instance.Insert(dto);
                return true;
            }

        }

        public override void Insert<T>(T dto)
        {
            _instance.Insert(dto);
        }

        public override bool Delete<T>(int id)
        {
            _instance.Delete<T>(id);
            return true;
        }

        public override void DeleteAll<T>()
        {
            _instance.DropTable<T>();
            _instance.CreateTable<T>();
        }


        public override IEnumerable<Type> GetMappedTypes()
        {
            return _instance.TableMappings.Select(q => q.MappedType);
        }

        public override void MapEntityToTable<T>()
        {
            
        }

        public override void RunInTransaction(Action transaction)
        {
            _instance.RunInTransaction(transaction);
        }



        protected override string GetDbPath(string dbVersion)
        {
#if UNITY_EDITOR
            var dbPath = string.Format(@"Assets/StreamingAssets/{0}", DatabaseName);
#else
            var dbWithVersion = DatabaseName + "_" + dbVersion;
            // check if file exists in Application.persistentDataPath
            var filepath = string.Format("{0}/{1}", Application.persistentDataPath, dbWithVersion);
            //var filepath = string.Format("{0}/{1}", Application.persistentDataPath, databaseName);

            Debug.Log("Search database: " + filepath);
            if (!File.Exists(filepath))
        {
            Debug.Log("Database not in Persistent path");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->

#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + DatabaseName);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath, true);
#elif UNITY_WP8
                var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath, true);

#elif UNITY_WINRT
		var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath, true);
		
#elif UNITY_STANDALONE_OSX
		var loadDb = Application.dataPath + "/Resources/Data/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
		// then save to Application.persistentDataPath
		File.Copy(loadDb, filepath, true);
#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + DatabaseName;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath, true);

#endif

            Debug.Log("Database written: " + filepath);
        }

        var dbPath = filepath;
#endif
            _instance = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            return dbPath;
        }
    }
}
