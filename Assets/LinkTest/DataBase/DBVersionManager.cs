using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.IO;
using System.Xml;
using System.Collections.Generic;
/// <summary>
/// 非强制更新的数据库资源版本控制管理类
/// author lgr
/// </summary>
public class DBVersionManager {
    private static CDBInstance mDBInstance = null;
    private static Dictionary<string, DataBaseInfo> mServerDB = null;
    public static CDBInstance GetDBInstance() {
        return mDBInstance; 
    }
    private static CDBInstance mLocalDBInstance = null;//本地数据库对象
    private static string DBPath = "";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    private static string DBLocalPath = Application.dataPath + "/LocalDB.db";//本地非强制更新的资源数据库路径
#elif UNITY_IPHONE
    private static string DBLocalPath = Application.persistentDataPath+"/LocalDB.db";
#elif UNITY_ANDROID
    private static string DBLocalPath = Application.persistentDataPath+"/LocalDB.db";
#endif
    
    /// <summary>
    /// 写入最新的服务器数据库
    /// </summary>
    /// <param name="bytes"></param>
    public static void SetServerXML(XmlDocument document)
    {
        mServerDB = new Dictionary<string, DataBaseInfo>();
        XmlNodeList list = document.SelectNodes("root/asset");
        foreach (XmlNode node in list)
        {
            string path = node.Attributes.GetNamedItem("path").Value;
            string md5 = node.Attributes.GetNamedItem("md5").Value;
            int size = int.Parse(node.Attributes.GetNamedItem("size").Value);
            int romote = int.Parse(node.Attributes.GetNamedItem("romote").Value);
            DataBaseInfo info = new DataBaseInfo();
            info.path = path;
            info.md5 = md5;
            info.size = size;
            info.romote = romote;
            mServerDB.Add(path, info);
        }
    }

    public static CDBInstance GetLocalDBInstance() {
        return mLocalDBInstance;
    }
    public static void CloseLocalDB() {
        if (mLocalDBInstance != null)
        {
            mLocalDBInstance.Close();
        }
        
    }
    public static void OpenLocalDB()
    {
        if (!File.Exists(DBLocalPath))
        {
            FileStream fs = File.Create(DBLocalPath);
            fs.Close();
            fs.Dispose();
            mLocalDBInstance = CDataBaseManager.GetDBInstance(DBLocalPath);
            if (mLocalDBInstance.BeginTransaction())
            {
                mLocalDBInstance.CreateTable("unforceTable",
                    new string[] {
						"path",
						"md5",
						"romote",
						"size" },

                    new string[] {
						"TEXT NOT NULL",
						"INTEGER NOT NULL UNIQUE",
						"INTEGER NOT NULL",
						"INTEGER NOT NULL" });
                mLocalDBInstance.EndTransaction();
            }
        }else{
            mLocalDBInstance = CDataBaseManager.GetDBInstance(DBLocalPath);  
        }
    }
    /// <summary>
    /// 对比版本，看看是否需要更新
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DBVersionEnum CompareVersion(string path)
    {
        //bool isNew = false;//是否是新的 是新的就需要web加载之后写入本地
        DataBaseInfo oldInfo = GetDataInfo(mLocalDBInstance, path);//本地资源版本
        if(oldInfo==null) {
            return DBVersionEnum.Insert;
        }
        DataBaseInfo newInfo = mServerDB[path];
        if (newInfo.size != oldInfo.size || newInfo.md5 != oldInfo.md5) {
            return DBVersionEnum.Update;
        }

        return DBVersionEnum.None;
    }

    public static DataBaseInfo GetDataInfo(string path)
    {
        DataBaseInfo newInfo = mServerDB[path];
        return newInfo;
    }
    /// <summary>
    /// 根据路径获取本地数据库信息
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DataBaseInfo GetDataInfo(CDBInstance db,string path) {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        string resVal = path.ToLower();

        SqliteDataReader reader = db.ExecuteQuery(string.Format(Sql_Query_Info, resVal));
        if (null != reader && reader.Read())
        {
            DataBaseInfo result = new DataBaseInfo();

            result.md5 = reader.GetString(0);
            result.romote = reader.GetInt32(1);
            result.size = reader.GetInt32(2);

            return result;
        }

        return null;
    }
    /// <summary>
    /// 写入信息
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="valueArray"></param>
    public static void InsertInfo(CDBInstance db,string tableName, string[] valueArray)
    {
        if (db.BeginTransaction())
        {
            db.InsertInto(tableName, valueArray);
            db.EndTransaction();
        }
        
        
    }
    /// <summary>
    /// 修改信息
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="cols"></param>
    /// <param name="colsvalues"></param>
    /// <param name="selectkey"></param>
    /// <param name="selectvalue"></param>
    public static void UpdateInfo(CDBInstance db, string tableName, string[] cols, string[] colsvalues, string selectkey, string selectvalue)
    {
        db.UpdateInto(tableName, cols, colsvalues, selectkey, selectvalue);
    }

    private static string Sql_Query_Info = "SELECT md5 , romote , size FROM unforceTable WHERE path = \'{0}\'";
}
public enum DBVersionEnum { 
    Insert,//写入
    Update,//修改
    None//已经是最新的，不变
}