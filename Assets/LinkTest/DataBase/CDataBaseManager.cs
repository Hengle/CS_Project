using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 数据库操作管理类,主要做非强制更新的资源的版本管理
/// author wlk
/// update lgr
/// </summary>
public static class CDataBaseManager
{
    public static CDBInstance GetDBInstance(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
      
        CDBInstance result = null;
        if (m_DicDataBase.TryGetValue(path, out result))
        {
            return result;
        }

        result = new CDBInstance(path);
        m_DicDataBase.Add(path, result);

        return result;
    }

    public static void CloseAll()
    {
        Dictionary<string, CDBInstance>.Enumerator itor = m_DicDataBase.GetEnumerator();
        while (itor.MoveNext())
        {
            itor.Current.Value.Close();
        }
        m_DicDataBase.Clear();
    }

    public static bool CloseDB(string path)
    {
        CDBInstance result = null;
        if (m_DicDataBase.TryGetValue(path, out result))
        {
            result.Close();
            m_DicDataBase.Remove(path);
            result = null;
            return true;
        }

        return false;
    }

    private static Dictionary<string, CDBInstance> m_DicDataBase = new Dictionary<string, CDBInstance>();

}