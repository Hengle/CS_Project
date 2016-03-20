using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
/// <summary>
/// 错误日志[一些特殊情况，严重错误的时候调用]
/// author wlk
/// updater lgr
/// </summary>
public static class LogHelp
{
    private static string s_LogPath;// = "DMTrace.log";
    private static byte[] s_EndLine = { 13, 10 };
    private static bool s_IsValie = true;
    private static bool Inited = false;

    private static void Init()
    {
        Inited = true;
        if (RuntimePlatform.IPhonePlayer == Application.platform
           || RuntimePlatform.Android == Application.platform)
        {
            s_LogPath = Application.persistentDataPath + "/DMTrace.log";
            s_IsValie = false;
        }
        else
        {
            s_LogPath = Application.dataPath + "/DMTrace.log";
        }

        System.IO.File.Delete(s_LogPath);

#if UNITY_EDITOR
        string logMetaPath = s_LogPath + ".meta";
        if (System.IO.File.Exists(logMetaPath))
        {
            System.IO.File.Delete(logMetaPath);
        }
#endif
    }

    public static void LogAuto(string strLog)
    {
        if (!Inited)
        {
            Init();
        }
#if UNITY_EDITOR

        LogConsole(strLog);
#else
			LogFile( strLog );
#endif

    }

    public static void LogError(string strError)
    {
        if (!Inited)
        {
            Init();
        }
#if UNITY_EDITOR

        string strTime = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]" + strError;
        Debug.LogError(strTime);
#else
			
			LogFile( strError );
#endif

    }

    public static void LogConsole(string strLog)
    {
        if (!Inited)
        {
            Init();
        }
        if (!s_IsValie)
        {
            return;
        }
        string strTime = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]" + strLog;
        Debug.Log(strTime);
    }

    public static void LogFile(byte[] buffer, int nOffset, int nLen)
    {
        if (!Inited)
        {
            Init();
        }
        FileStream writeStream = null;

        if (System.IO.File.Exists(s_LogPath))
        {
            writeStream = new FileStream(s_LogPath, FileMode.Append, FileAccess.Write, FileShare.Write);
        }
        else
        {
            writeStream = new FileStream(s_LogPath, FileMode.CreateNew, FileAccess.Write, FileShare.Write);
        }

        string strTime = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "]";
        byte[] valTime = System.Text.Encoding.Default.GetBytes(strTime);
        writeStream.Write(valTime, 0, valTime.Length);

        writeStream.Write(buffer, nOffset, nLen);
        writeStream.Write(s_EndLine, 0, s_EndLine.Length);

        writeStream.Flush();
        writeStream.Close();
    }

    public static void LogFile(string strVal)
    {
        return;
        if (!Inited)
        {
            Init();
        }
        byte[] val = System.Text.Encoding.Default.GetBytes(strVal);
        LogFile(val, 0, val.Length);
    }
}