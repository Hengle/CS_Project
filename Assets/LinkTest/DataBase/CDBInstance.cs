using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System;
using System.IO;
/// <summary>
/// 数据库操作
/// author wlk
/// update lgr
/// </summary>
public class CDBInstance
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    /*
	public	CDBInstance( string path, bool bFull )
	{
		m_bUseFullPath = bFull;
		m_FirstOpen   = true;
		m_StrPath     = path;
		OpenDB();
	}
     * */
#endif
    /// <summary>
    /// 获取数据库操作对象
    /// </summary>
    /// <param name="path">数据库连接路径，根据不同平台传不同路径值</param>
    public CDBInstance(string path)
    {
#if 	UNITY_EDITOR || UNITY_STANDALONE_WIN
		//m_bUseFullPath = false;
#endif
        m_FirstOpen = true;
        m_StrPath = path;
        OpenDB(); 
    }

    public bool IsFirstOpen()
    {
        return m_FirstOpen;
    }

    public bool IsValid()
    {
        return (null != m_SqlInstance) ? true : false;
    }

    public SqliteDataReader QueryTable(string tableName)
    {
        return ExecuteQuery("SELECT * FROM " + tableName);
    }

    public int DeleteItems(string tableName, string[] cols, string[] colsvalues)
    {
        string query = "DELETE FROM " + tableName + " WHERE " + cols[0] + " = " + colsvalues[0];

        for (int i = 1; i < colsvalues.Length; ++i)
        {
            query += " or " + cols[i] + " = " + colsvalues[i];
        }

        return ExecuteNoneQuery(query);
    }
	
    public bool BeginTransaction()
    {
        if (!IsValid())
        {
            
            return false;
        }

        ClearTransAction();

        m_SqlTransaction = m_SqlInstance.BeginTransaction();
        return true;
    }

    public void EndTransaction()
    {
        if (null != m_SqlTransaction)
        {
            try
            {
                m_SqlTransaction.Commit();
                m_SqlTransaction.Dispose();
                m_SqlTransaction = null;
            }
            catch (Exception e)
            {
                LogHelp.LogFile(e.ToString());
            }
        }
    }

    public int InsertInto(string tableName, string[] valueArray)
    {
        if (string.IsNullOrEmpty(tableName) || null == valueArray || 0 == valueArray.Length)
        {
            return 0;
        }

        string quertyStr = "INSERT INTO " + tableName + " VALUES (" + valueArray[0];
        for (int lCnt = 1; lCnt < valueArray.Length; ++lCnt)
        {
            quertyStr += ", " + valueArray[lCnt];
        }
        quertyStr += ")";
        return ExecuteNoneQuery(quertyStr);
    }

    public int UpdateInto(
        string tableName,
        string[] cols,
        string[] colsvalues,
        string selectkey,
        string selectvalue)
    {
        string query = "UPDATE " + tableName + " SET " + cols[0] + " = " + colsvalues[0];

        for (int i = 1; i < colsvalues.Length; ++i)
        {
            query += ", " + cols[i] + " = " + colsvalues[i];
        }
        query += " WHERE " + selectkey + " = " + selectvalue + " ";

        return ExecuteNoneQuery(query);
    }

    public int InsertIntoSpecific(string tableName, string[] cols, string[] values)
    {
        if (cols.Length != values.Length)
        {
            return 0;
        }

        string query = "INSERT INTO " + tableName + "(" + cols[0];
        for (int i = 1; i < cols.Length; ++i)
        {
            query += ", " + cols[i];
        }
        query += ") VALUES (" + values[0];
        for (int i = 1; i < values.Length; ++i)
        {
            query += ", " + values[i];
        }
        query += ")";

        return ExecuteNoneQuery(query);
    }


    public int DeleteTableItems(string tableName)
    {
        string query = "DELETE FROM " + tableName;

        return ExecuteNoneQuery(query);
    }

    public int CreateTable(string tableName, string[] col, string[] colType)
    {
        if (col.Length != colType.Length)
        {
            return 0;
        }

        string query = "CREATE TABLE " + tableName + " (" + col[0] + " " + colType[0];

        for (int i = 1; i < col.Length; ++i)
        {
            query += ", " + col[i] + " " + colType[i];
        }
        query += ")";

        return ExecuteNoneQuery(query);
    }

    public int GetCount(string tableName)
    {
        string query = "SELECT count(*) FROM " + tableName;
        SqliteDataReader reader = ExecuteQuery(query);
        if (null != reader && reader.Read())
        {
            return reader.GetInt32(0);
        }

        return 0;
    }

    public SqliteCommand CreateCommand(string sqlLan)
    {
        if (!IsValid())
        {
            return null;
        }

        SqliteCommand command = new SqliteCommand(sqlLan, m_SqlInstance);
        return command;
    }

    public SqliteDataReader SelectWhere(
        string tableName,
        string[] items,
        string[] col,
        string[] operation,
        string[] values)
    {
        if (col.Length != operation.Length || operation.Length != values.Length)
        {
            return null;
        }

        string query = "SELECT " + items[0];

        for (int i = 1; i < items.Length; ++i)
        {
            query += ", " + items[i];
        }

        query += " FROM " + tableName + " WHERE " + col[0] + operation[0] + "'" + values[0] + "' ";

        for (int i = 1; i < col.Length; ++i)
        {
            query += " AND " + col[i] + operation[i] + "'" + values[i] + "' ";
        }

        return ExecuteQuery(query);
    }

    public SqliteDataReader SelectWhere(
        string tableName,
        string[] items,
        string col,
        string operation,
        string strValue)
    {
        string query = "SELECT " + items[0];

        for (int i = 1; i < items.Length; ++i)
        {
            query += ", " + items[i];
        }

        query += " FROM " + tableName + " WHERE " + col + operation + "'" + strValue + "' ";

        return ExecuteQuery(query);
    }


    public SqliteDataReader SelectWhere(
        string tableName,
        string item,
        string col,
        string operation,
        string strValue)
    {
        string query = "SELECT " + item;

        query += " FROM " + tableName + " WHERE " + col + operation + "'" + strValue + "' ";

        return ExecuteQuery(query);
    }

    public int ExecuteNoneQuery(string sqlLan)
    {
        if (null == m_SqlInstance || string.IsNullOrEmpty(sqlLan))
        {
            return 0;
        }

        try
        {
            SqliteCommand mycommand = new SqliteCommand(m_SqlInstance) { CommandText = sqlLan };
            int result = mycommand.ExecuteNonQuery();
            mycommand.Dispose();
            mycommand = null;
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            LogHelp.LogFile(e.ToString());
            return 0;
        }
    }

    public SqliteDataReader ExecuteQuery(string strQuery)
    {
        if (null == m_SqlInstance || string.IsNullOrEmpty(strQuery))
        {
            return null;
        }
        
        ClearReader();

        try
        {
            SqliteCommand tempSqlCommand = m_SqlInstance.CreateCommand();
            tempSqlCommand.CommandText = strQuery;
            m_SqlReader = tempSqlCommand.ExecuteReader();
            tempSqlCommand.Dispose();
            tempSqlCommand = null;

            return m_SqlReader;
        }
        catch (Exception e)
        {
            LogHelp.LogFile(e.ToString());
            return null;
        }
    }


    public void ReOpen()
    {
        if (null != m_SqlInstance)
        {
            return;
        }

        Close();
        OpenDB();
    }

    public void Close()
    {
        ClearTransAction();

        ClearReader();

        if (null != m_SqlInstance)
        {
            m_SqlInstance.Close();
            m_SqlInstance.Dispose();
            m_SqlInstance = null;
        }
    }

    public void Destroy()
    {
        Close();
    }

    ////////////////////////////////////////////////////////////

    private void OpenDB()
    {
        m_SqlReader = null;
        if (File.Exists(m_StrPath))
        {
            m_FirstOpen = false;
        }
        else {
            m_FirstOpen = true;
        }

        try
        {
            m_SqlInstance = new SqliteConnection("URI=file:" + m_StrPath);
            //m_SqlInstance = new SqliteConnection(m_StrPath);
            //Debug.LogError("URI=file:" + m_StrPath);
            m_SqlInstance.Open();
        }
        catch (Exception e)
        {
            m_SqlInstance = null;
            LogHelp.LogFile(e.ToString());
        }
    }

    private CDBInstance()
    {
        m_SqlTransaction = null;
        m_SqlInstance = null;
        m_SqlReader = null;
        m_StrPath = string.Empty;
        m_FirstOpen = false;

#if 	UNITY_EDITOR || UNITY_STANDALONE_WIN
        //m_bUseFullPath = false;
#endif
    }

    private void ClearTransAction()
    {
        if (null != m_SqlTransaction)
        {
            m_SqlTransaction.Dispose(); 
            m_SqlTransaction = null;
        }
    }

    public void ClearReader()
    {
        if (null != m_SqlReader)
        {
            m_SqlReader.Close();
            m_SqlReader.Dispose();
            m_SqlReader = null;
        }
    }

    private bool m_FirstOpen;
    private string m_StrPath;
    private SqliteDataReader m_SqlReader;
    private SqliteConnection m_SqlInstance;
    private SqliteTransaction m_SqlTransaction;

#if UNITY_EDITOR|| UNITY_STANDALONE_WIN
    //private	bool m_bUseFullPath;
#endif
}