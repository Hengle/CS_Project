
#if UNITY_EDITOR


using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;


namespace DMCore.Build
{
    public static	class CBuildIOHelp
    {
        public static void CopyDirectory( string sourceDirName, string destDirName )
        {
			if ( ! Directory.Exists(sourceDirName) )
            {
                Debug.LogError("Src Dir is NOT exist !! name : " + sourceDirName);
                return;
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
                File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
            }

            if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
                destDirName = destDirName + Path.DirectorySeparatorChar;

            string[] files = Directory.GetFiles(sourceDirName);
            foreach (string file in files)
            {
                File.Copy(file, destDirName + Path.GetFileName(file), true);
                File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
            }

            string[] dirs = Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
            {
                CopyDirectory(dir, destDirName + Path.GetFileName(dir));
            }
        }

        public static bool	DeleteDirectory( string path )
        {
            if ( System.IO.Directory.Exists( path ) )
            {
            	System.IO.Directory.Delete( path, true );
				return	true;
            }

			return	false;
        }

		public static bool _ToInt(string strValue, ref int nResult)
		{
			if (null == strValue || 0 == strValue.Length)
			{
				return false;
			}

			try
			{
				nResult = System.Convert.ToInt32(strValue);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static int _GetXmlElemInt(XmlElement elemNode, string key, int nDefault)
		{
			if (null == elemNode)
			{
				return nDefault;
			}

			string strValue = elemNode.GetAttribute(key);

			int nResult = 0;
			if (_ToInt(strValue, ref nResult))
			{
				return nResult;
			}
			else
			{
				return nDefault;
			}
		}

		public static string _GetXmlElemString(XmlElement elemNode, string key)
		{
			return _GetXmlElemString(elemNode, key, "");
		}

		public static string _GetXmlElemString(XmlElement elemNode, string key, string defaultVal)
		{
			if (null == elemNode || string.IsNullOrEmpty(key))
			{
				return defaultVal;
			}

			return elemNode.GetAttribute(key);
		}
    }
}


#endif
