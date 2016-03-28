
//#if UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using DMCore.Build;


public	class CBuildCfg
{
	public	CBuildCfg()
	{
		m_DefaultLanguage = "CN_SIMPLIFIED";
		m_bEnalbe = true;
		m_bDisableShared = false;
	}

	public	string	m_Name;
	public	string	m_DefaultLanguage;
	public	string	m_PluginPath;
	public	string	m_APKBaseName;
	public	string	m_ProductName;
	public	string	m_BoundleIdentifier;
	public	string	m_iconPath;
	public	bool	m_bEnalbe;
	public	bool	m_bDisableShared;
}

public class CBuildTool : EditorWindow
{
	private	static	bool	s_Showed = false;
	
	private	static	string	m_sdkDefault;
	
	private	List< CBuildCfg >	m_cfgList;
	private	string				m_rootPath;
	private	string				m_lastbuildVersion;
	private	string				m_buildVersion;
	
	private	bool				m_bOpenGray;
	private	bool				m_bOpenRelease;

	private	List< int >			m_listGray;
	private	List< int >			m_listRelease;
	
	[MenuItem("BuildTool/ShowBuildPanel")]
    public static void ShowBuildPanel()
    {
		if( CBuildTool.s_Showed )
		{
			Debug.LogError( "Allready Show Build Panel !~~" );
			return;
		}
        
        /*
		if ( !TextUIExport.HideAllPrefab() || !TextUIExport.CheckPrefabValidation() )
		{
			Debug.LogError( "Please check prefab resource!!!" );
			return;
		}
        */ 
		string rootPath = string.Empty;
		List< CBuildCfg > result = SelectCfgList( ref rootPath );

		if( null == result || 0 >= result.Count )
		{
			Debug.LogError( "Nothing To Build!~~" );
			return;
		}
		
		Vector2 v2Size = new Vector2( 400.0f, 600.0f );
		CBuildTool window = (CBuildTool) EditorWindow.GetWindow ( typeof (CBuildTool) );		
		window.m_listGray = new List< int >();
		window.m_listRelease = new List<int>();

		for( int lCnt = 0; lCnt < result.Count; ++lCnt )
		{
			if( 0 <= result[lCnt].m_Name.IndexOf( "GRAY_" ) )
			{
				window.m_listGray.Add( lCnt );
			}
			else if( 0 <= result[lCnt].m_Name.IndexOf( "RELEASE_" ) )
			{
				window.m_listRelease.Add( lCnt );
			}
		}

		window.title = "Build Panel";
		window.m_rootPath = rootPath;
		window.m_cfgList = result;
		window.maxSize = v2Size;
		window.minSize = v2Size;
		window.m_bOpenGray = false;
		window.m_bOpenRelease = false;
		CBuildTool.s_Showed = true;
		window.OnInit();
		window.Show();
		
	}


	void Start ()
	{
		m_buildVersion = string.Empty;
	}
	
	void OnDestroy()
	{
		CBuildTool.s_Showed = false;
		m_cfgList = null;
	}
	
	void OnInit()
	{
		if( null != m_cfgList )
		{			
			for( int lCnt = 0; lCnt < m_cfgList.Count; ++lCnt )
			{
				m_cfgList[lCnt].m_bEnalbe = false;
			}
		}

		m_lastbuildVersion = "0.1.0";
		
		int[] version = new int[3];
		version[0] = 0;
		version[1] = 1;
		version[2] = 0;

		string apkPath = GetAPkReleaseRoot();
		if( System.IO.Directory.Exists( apkPath ) )
		{
			string[] dirs = System.IO.Directory.GetDirectories( apkPath );
			
			for( int lCnt = 0; lCnt < dirs.Length; ++lCnt )
			{
				string[] vals = dirs[lCnt].Split( '.' );
				
				int[] curVersion = new int[3] { 0, 0, 0 };
				
				if( 0 < vals.Length )
				{
					CBuildIOHelp._ToInt( vals[0], ref curVersion[0] );
				}
				if( 1 < vals.Length )
				{
					CBuildIOHelp._ToInt( vals[1], ref curVersion[1] );
				}
				if( 2 < vals.Length )
				{
					CBuildIOHelp._ToInt( vals[2], ref curVersion[2] );
				}

				for( int j = 0; j < 3; ++j )
				{
					if( version[j] < curVersion[j] )
					{
						version[j] = curVersion[j];

						for( int i = j+1; i < 3; ++i )
						{
							version[i] = curVersion[i];
						}
						break;
					}
				}
			}
						
			m_lastbuildVersion = string.Format( "{0:D}.{1:D}.{2:D}", version[0], version[1], version[2] );
		}
		else
		{
			m_lastbuildVersion = PlayerSettings.bundleVersion;
		}
		
		m_buildVersion     = m_lastbuildVersion;
	}
	
	void OnGUI () 
	{
		GUI.Label( new Rect( 0.0f, 0.0f, 400.0f, 36.0f ), 
			string.Format( "Last Version => [{0}]", m_lastbuildVersion ) );
		
		GUI.Label( new Rect( 0.0f, 26.0f, 400.0f, 36.0f ), 
			string.Format( "Target Version => [{0}]", m_buildVersion ) );
		
		if( null == m_cfgList || 0 == m_cfgList.Count )
		{
			GUI.Label( new Rect( 0.0f, 360.0f, 400.0f, 36.0f ), "Not Exist Any Build Item !! " );
			
			return;
		}
		
		float fYLabor = 50.0f;
		
		Color oldColor = GUI.backgroundColor;

		if( 0 == string.Compare( m_lastbuildVersion, m_buildVersion ) )
		{
			GUI.backgroundColor = Color.green;
			if( GUI.Button(  new Rect( 350.0f, fYLabor, 50.0f, 44.0f ), "++" ) )
			{
				int indexVal = m_lastbuildVersion.LastIndexOf( '.' );
				if( 0 <= indexVal && ( 1 + m_lastbuildVersion.Length ) > indexVal )
				{
					int lastVer = 0;
					CBuildIOHelp._ToInt( m_lastbuildVersion.Substring( indexVal+1, m_lastbuildVersion.Length - indexVal - 1 ), ref lastVer );
					m_buildVersion = m_lastbuildVersion.Substring( 0, indexVal + 1 ) + ( lastVer + 1 ).ToString();
				}
			}
		}
		else
		{
			GUI.backgroundColor = Color.red;
			if( GUI.Button(  new Rect( 0.0f, fYLabor, 50.0f, 44.0f ), "HOLD" ) )
			{
				m_buildVersion = m_lastbuildVersion;
			}			
		}
		
		GUI.backgroundColor = Color.gray;
		m_buildVersion = GUI.TextField( new Rect( 80.0f, fYLabor, 240.0f, 44.0f ), m_buildVersion );
		
		// Select Build Target !!
		float	flaborPosYMax  = 120.0f;
		int 	laborIndex = 0;
		
		for( int lCnt = 0; lCnt < m_cfgList.Count; ++lCnt )
		{
			if( m_listGray.Contains( lCnt ) || m_listRelease.Contains( lCnt ) )
			{
				continue;
			}

			float fXPos = ( laborIndex % 3 ) * 120.0f + 20.0f;
			float fYPos = ( laborIndex / 3 ) * 50.0f + 120.0f;

			flaborPosYMax = Mathf.Max( flaborPosYMax, fYPos );
			
			++laborIndex;
			
			bool bEnable = m_cfgList[ lCnt ].m_bEnalbe;
			
			GUI.backgroundColor = bEnable ? Color.green : Color.red;

			if( GUI.Button( new Rect( fXPos, fYPos, 100.0f, 40.0f ), m_cfgList[ lCnt ].m_Name ) )
			{
				m_cfgList[ lCnt ].m_bEnalbe = ! bEnable;
			}
		}
		
		flaborPosYMax += 50.0f;
		
		if( 0 < m_listGray.Count )
		{
			GUI.backgroundColor = m_bOpenGray ? Color.green : Color.red;
			int iGray = 0;
			if( GUI.Button( new Rect( 0.0f, flaborPosYMax, 200.0f, 40.0f ), "All Gray=>" + m_listGray.Count.ToString() ) )
			{
				m_bOpenGray = ! m_bOpenGray;
				
				for( ; iGray < m_listGray.Count; ++iGray )
				{
					int indexGray = m_listGray[ iGray ];
					m_cfgList[ indexGray ].m_bEnalbe = m_bOpenGray;
				}
			}

			flaborPosYMax += 50.0f;
			float fYMaxOld = flaborPosYMax;

			if( m_bOpenGray )
			{
				for( iGray = 0; iGray < m_listGray.Count; ++iGray )
				{					
					float fXPos = ( iGray % 3 ) * 120.0f + 20.0f;
					float fYPos = ( iGray / 3 ) * 50.0f + fYMaxOld;
					flaborPosYMax = Mathf.Max( flaborPosYMax, fYPos );
					
					int indexGray = m_listGray[ iGray ];
					GUI.backgroundColor = m_cfgList[ indexGray ].m_bEnalbe ? Color.green : Color.red;
					
					string grayShowName = m_cfgList[ indexGray ].m_Name;
					grayShowName = grayShowName.Substring( 5, grayShowName.Length - 5 );
					if( GUI.Button( new Rect( fXPos, fYPos, 100.0f, 40.0f ), grayShowName ) )
					{
						m_cfgList[ indexGray ].m_bEnalbe = ! m_cfgList[ indexGray ].m_bEnalbe;
					}
				}
				
				flaborPosYMax += 30.0f;
			}
		}
		
		flaborPosYMax += 50.0f;
		
		if( 0 < m_listRelease.Count )
		{
			GUI.backgroundColor = m_bOpenRelease ? Color.green : Color.red;
			int iRelease = 0;
			if( GUI.Button( new Rect( 0.0f, flaborPosYMax, 200.0f, 40.0f ), "All Release=>" + m_listRelease.Count.ToString() ) )
			{
				m_bOpenRelease = ! m_bOpenRelease;
				
				for( ; iRelease < m_listRelease.Count; ++iRelease )
				{
					int indexRelease = m_listRelease[ iRelease ];
					m_cfgList[ indexRelease ].m_bEnalbe = m_bOpenRelease;
				}
			}

			flaborPosYMax += 50.0f;
			float fYMaxOld = flaborPosYMax;

			if( m_bOpenRelease )
			{
				for( iRelease = 0; iRelease < m_listRelease.Count; ++iRelease )
				{					
					float fXPos = ( iRelease % 3 ) * 120.0f + 20.0f;
					float fYPos = ( iRelease / 3 ) * 50.0f + fYMaxOld;
					flaborPosYMax = Mathf.Max( flaborPosYMax, fYPos );
					
					int indexRelease = m_listRelease[ iRelease ];

					GUI.backgroundColor = m_cfgList[ indexRelease ].m_bEnalbe ? Color.green : Color.red;
					
					string releaseShowName = m_cfgList[ indexRelease ].m_Name;
					releaseShowName = releaseShowName.Substring( 8, releaseShowName.Length - 8 );
					
					if( GUI.Button( new Rect( fXPos, fYPos, 100.0f, 40.0f ), releaseShowName ) )
					{
						m_cfgList[ indexRelease ].m_bEnalbe = ! m_cfgList[ indexRelease ].m_bEnalbe;
					}
				}
				
				flaborPosYMax += 30.0f;
			}
		}
				
		GUI.backgroundColor = oldColor;
		
		int validCount = 0;
		for( int lCnt = 0; lCnt < m_cfgList.Count; ++lCnt )
		{
			validCount += m_cfgList[lCnt].m_bEnalbe ? 1 : 0;
		}
		
		Vector2 sizeQuote = base.minSize;
		sizeQuote.y = flaborPosYMax + 80.0f;
		base.minSize = sizeQuote;
		base.maxSize = sizeQuote;
		
		if( GUI.Button( new Rect( 0.0f, flaborPosYMax + 30.0f, 400.0f, 44.0f ), "BUILD TARGET => " + validCount.ToString() ) )
		{
			if( string.IsNullOrEmpty( m_buildVersion ) )
			{
				Debug.LogError( "Version Can Not Empty!!" );
				return;
			}

			if( 0 == validCount )
			{
				Debug.LogError( "Nothing To Build!!" );
				return;
			}

			for( int j = 0; j < m_buildVersion.Length; ++j )
			{
				char charVal = m_buildVersion[j];
				if( ! ( '.' == charVal || ( (int)'0' <= (int)charVal && (int)'9' >= (int)charVal ) ) )
				{
					Debug.LogError( "Invalid Version => " + m_buildVersion );
					return;
				}
			}

			for( int i = 0; i < m_cfgList.Count; ++i )
			{
				if( m_cfgList[i].m_bEnalbe )
				{
					BuildPlatform( m_cfgList[i], m_rootPath, m_buildVersion );
				}
			}

			Debug.Log( "Build End!!!" );
			Debug.Log( "Build End!!!" );
			
			m_cfgList = null;
			Close();

			CBuildTool.s_Showed = false;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////
	
	private	static	string				GetAPkReleaseRoot()
	{
		string apkPath = Application.dataPath;
		apkPath = apkPath.Replace( '\\', '/' );
		int index = apkPath.LastIndexOf( '/' );
		apkPath = apkPath.Substring( 0, index );
		
		return apkPath + "/Release";
	}
	
	private	static List< CBuildCfg >	SelectCfgList( ref string strSDKPath )
	{
		strSDKPath = EditorUtility.OpenFolderPanel( "Select Plugin Root!", "", "" );
		if( string.IsNullOrEmpty( strSDKPath ) || ! System.IO.Directory.Exists( strSDKPath )  )
		{
			Debug.LogError( "[Error]: Can't Find Plugin Base Root !!" );
			return	null;
		}
		
		string strFullPath = strSDKPath + "/buildCfg_Android.txt";
		if( ! System.IO.File.Exists( strFullPath ) )
		{
			Debug.LogError( "[Error]: Can't Find buildCfg_Android.txt !!" );
			return	null;				
		}
		
		List< CBuildCfg > listResult = null;
		
		m_sdkDefault = string.Empty;
		
		if( ! string.IsNullOrEmpty( strFullPath ) && System.IO.File.Exists( strFullPath ) )
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load( strFullPath );
				
	            XmlNode root = ( null != doc ) ? doc.FirstChild : null;
				
				XmlElement rootElement = ( null != root ) ? ( XmlElement ) root : null;
				if( null != rootElement )
				{
					m_sdkDefault = CBuildIOHelp._GetXmlElemString( rootElement, "sharedSDK" );
				}
				
	            XmlNodeList nodeList = ( null != root ) ? root.ChildNodes : null;
				if( null != nodeList && 0 < nodeList.Count )
				{
					listResult = new List< CBuildCfg >();
					
					for( int lCnt = 0; lCnt < nodeList.Count; ++lCnt )
					{
						CBuildCfg cfg = new CBuildCfg();
						
						XmlElement element = (XmlElement) nodeList.Item( lCnt );

						cfg.m_Name    			= CBuildIOHelp._GetXmlElemString( element, "name" );
						cfg.m_bEnalbe           = false;
						
	            		XmlNodeList nodeSubList = element.ChildNodes;
						for( int i = 0; i < nodeSubList.Count; ++i )
						{
							XmlElement subElement = (XmlElement) nodeSubList.Item( i );
							
							if( 0 == subElement.Name.CompareTo( "Product" ) )
							{
								cfg.m_ProductName   = CBuildIOHelp._GetXmlElemString( subElement, "val" );
							}
							if( 0 == subElement.Name.CompareTo( "BoundleID" ) )
							{
								cfg.m_BoundleIdentifier = CBuildIOHelp._GetXmlElemString( subElement, "val" );
							}
							if( 0 == subElement.Name.CompareTo( "iconPath" ) )
							{
								cfg.m_iconPath = CBuildIOHelp._GetXmlElemString( subElement, "val" );
							}
							if( 0 == subElement.Name.CompareTo( "Plugin" ) )
							{
								cfg.m_PluginPath = CBuildIOHelp._GetXmlElemString( subElement, "val" );
							}
							if( 0 == subElement.Name.CompareTo( "ApkName" ) )
							{
								cfg.m_APKBaseName = CBuildIOHelp._GetXmlElemString( subElement, "val" );
							}
							if( 0 == subElement.Name.CompareTo( "DisableShared" ) )
							{
								int disableShared = CBuildIOHelp._GetXmlElemInt( subElement, "val", 0 );
								cfg.m_bDisableShared = ( 0 != disableShared ) ? true : false;
							}
							if( 0 == subElement.Name.CompareTo( "language" ) )
							{
								cfg.m_DefaultLanguage = CBuildIOHelp._GetXmlElemString( subElement, "val" );
							}
						}

						listResult.Add( cfg );
					}
				}
			}
	        catch ( Exception ex )
	        {
	            Debug.LogError( "XML Cfg Error : " + ex.StackTrace );
	        }

			doc = null;
		}
		
		return	listResult;	
	}
	
	private	static	CBuildCfg	GetBuildCfg( string keyName, ref string rootPath )
	{
		List< CBuildCfg > result = SelectCfgList( ref rootPath );
		
		if( null == result || 0 >= result.Count )
		{
			Debug.LogError( "Nothing To Build!~~" );
			return	null;
		}
		
		for( int lCnt = 0; lCnt < result.Count; ++lCnt )
		{
			if( 0 == result[lCnt].m_Name.CompareTo( keyName ) )
			{
				return	result[lCnt];
			}
		}
		
		return	null;
	}

	private	static void		ResetSettingCfg( string cfgName, string cfgLanguage )
	{
		string xmlPath = Application.dataPath + "/Resources/GameSetting/Setting.xml";

		XmlDocument doc = null;

		try
		{
			bool bChannelOK = false;
			bool bLanguageOK = false;

			doc = new XmlDocument();
			doc.Load( xmlPath );

			#region Type_1: XmlReader (XMLµÄ×¢ÊÍ»á¶ªÊ§)

// 			XmlNode root = ( null != doc ) ? doc.SelectSingleNode("root") : null;
// 			XmlNodeList nodeList = ( null != root ) ? root.ChildNodes : null;
// 			if ( null == nodeList )
// 			{
// 				return;
// 			}
// 			
// 			for ( int lCnt = 0; lCnt < nodeList.Count; ++lCnt )
// 			{
// 				XmlNode sequenceXml = nodeList.Item( lCnt );
// 				
// 				string sequenceName =  sequenceXml.Attributes["name"].Value;
// 
// 				if ( ! string.IsNullOrEmpty(sequenceName) && 0 == sequenceName.CompareTo( "env_package" ) )
// 				{
// 					bChannelOK = true;
// 					sequenceXml.Attributes["value"].Value = cfgName;
// 					break;
// 				}
// 
// 				if ( ! string.IsNullOrEmpty(sequenceName) && 0 == sequenceName.CompareTo( "env_language" ) )
// 				{
// 					bLanguageOK = true;
// 					sequenceXml.Attributes["value"].Value = cfgName;
// 					break;
// 				}
// 			}
// 
// 			if (bChannelOK && bLanguageOK)
// 			{
// 				doc.Save( xmlPath );
// 			}

			#endregion

			#region Type_2: XPath ()

			// set channel.
			XmlElement packageNode = doc.SelectSingleNode("/root/node[@name='env_package']") as XmlElement;
			if (null != packageNode)
			{
				bChannelOK = true;
				packageNode.SetAttribute("value", cfgName);
			}
			// set language.
			XmlElement languageNode = doc.SelectSingleNode("/root/node[@name='env_language']") as XmlElement;
			if (null != languageNode)
			{
				bLanguageOK = true;
				languageNode.SetAttribute("value", cfgLanguage);
			}

			if (bChannelOK && bLanguageOK)
			{
				doc.Save( xmlPath );
			}

			#endregion
		}
		catch (Exception e)
		{
			Debug.LogError( e.ToString() );
		}
		finally
		{
			doc = null;
		}
	}

    private static bool	BuildPlatform( CBuildCfg cfg, string pluginRoot, string BoundleVersion )
    {
		if( string.IsNullOrEmpty( cfg.m_Name ) )
		{
			Debug.LogError( "Cfg Node \'Name\' Can NOT EMPTY !~~" );
			return false;
		}

		// Step 1 : Player Setting
        PlayerSettings.productName	 	= cfg.m_ProductName;
        PlayerSettings.bundleIdentifier = cfg.m_BoundleIdentifier;
		PlayerSettings.bundleVersion    = BoundleVersion;

		// Step 2 : Icon
		if( ! string.IsNullOrEmpty( cfg.m_iconPath ) )
		{
			string iconFullPath  = string.Format( "{0}/{1}" , pluginRoot, cfg.m_iconPath );
            string iconLocalpath = Application.dataPath + "/Texture/AppIcon";

            CBuildIOHelp.CopyDirectory( iconFullPath, iconLocalpath );
		}

		// Step 3 : Local Setting CFG

//		ResetSettingCfg( cfg.m_Name, cfg.m_DefaultLanguage );
		
		// Step 4 : Copy APK
		
		// Step 4.1 : Shared SDK
		string strPathDefaultSDK = string.Empty;
		if(  ! cfg.m_bDisableShared && ! string.IsNullOrEmpty( m_sdkDefault ) )
		{
			strPathDefaultSDK = string.Format( "{0}/{1}" , pluginRoot, m_sdkDefault );
		}
		
		// Step 4.2 : Special SDK
		string strPathSpecialSDK = string.Empty;
		if( ! string.IsNullOrEmpty( cfg.m_PluginPath ) )
		{
			strPathSpecialSDK = string.Format( "{0}/{1}" , pluginRoot, cfg.m_PluginPath );
		}

		CopySDK( strPathDefaultSDK, strPathSpecialSDK );
		
		
		// Step 4.3 Remind XMl Manifest
		
//		string xmlPath = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";
//		if( System.IO.File.Exists( xmlPath ) )
//		{
//			XmlDocument	doc = new XmlDocument();
//			
//			doc.Load( xmlPath );
//			
//			bool bFindRoot = false;
//            XmlNode root = doc.FirstChild;
//			while( null != root )
//			{
//				if( 0 == string.Compare( "manifest", root.Name ) )
//				{
//					bFindRoot = true;
//					break;
//				}
//				else
//				{
//					root = root.NextSibling;
//				}
//			}
//			
//			if( ! bFindRoot )
//			{
//				return false;
//			}
//			
//            XmlNodeList nodeList = ( null != root ) ? root.ChildNodes : null;
//            if ( null == nodeList )
//            {
//				return false;
//            }
//			
//			bool bFind = false;
//			
//			for( int lCnt = 0; lCnt < nodeList.Count; ++lCnt )
//			{
//				XmlElement curElement = nodeList.Item( lCnt ) as XmlElement;
//				
//				if( null != curElement &&  0 == string.Compare( "application", curElement.Name.ToLower() ) )
//				{
//           			XmlNodeList serviceOrActivityList = curElement.ChildNodes;
//					
//					for( int i = 0; i < serviceOrActivityList.Count; ++i )
//					{
//						XmlElement serviceElement = serviceOrActivityList.Item( i ) as XmlElement;
//						
//						if(	null != serviceElement 
//							&&  0 == string.Compare( "service", serviceElement.Name.ToLower() )
//							&&	0 == string.Compare( "com.phoneu.service.phoneuservice",
//													  CBuildCopyFile._GetXmlElemString( serviceElement, "android:name" ).ToLower() ) )
//						{
//           					XmlNodeList filterList = serviceElement.ChildNodes;
//							
//							for( int k = 0; k < filterList.Count; ++k )
//							{
//								XmlElement Element0 = filterList.Item( k ) as XmlElement;
//								
//								if( null != Element0 && 0 == string.Compare( "intent-filter", Element0.Name.ToLower() ) )
//								{
//           							XmlNodeList filterListSub = Element0.ChildNodes;
//									
//									for( int j = 0; j < filterListSub.Count; ++j )
//									{
//										XmlElement Element1 = filterListSub.Item( j ) as XmlElement;
//										
//										if( null != Element1 && 0 == string.Compare( "category", Element1.Name.ToLower() ) )
//										{
//											Element1.SetAttribute( "android:name", cfg.m_BoundleIdentifier );
//											
//											bFind = true;
//											break;
//										}
//										
//									}									
//								}
//								
//								if( bFind )
//								{
//									break;
//								}								
//							}							
//						}
//
//						if( bFind )
//						{
//							break;
//						}
//					}
//					
//					if( bFind )
//					{
//						break;
//					}
//				}
//				
//				if( bFind )
//				{
//					break;
//				}
//			}
//			
//			
//			if( bFind )
//			{
//				doc.Save( xmlPath );
//
//				doc = null;
//			}
//			else
//			{
//				Debug.LogError( "Can't Find XML m_BoundleIdentifier" );
//				
//				doc = null;
//				return false;
//			}
//		}

		
		// Step 5 : Build APK
		string apkName = BoundleVersion;
		apkName = apkName.Replace( '.', '_' );
		apkName = cfg.m_APKBaseName + apkName;
		
		string subPath = GetAPkReleaseRoot();
		if( ! System.IO.Directory.Exists( subPath ) )
		{
			System.IO.Directory.CreateDirectory( subPath );
		}
		
		subPath = subPath + "/" + BoundleVersion;
		if( ! System.IO.Directory.Exists( subPath ) )
		{
			System.IO.Directory.CreateDirectory( subPath );
		}

        return true;
        if ( ! BuildAPK( subPath, apkName ) )
        {
            Debug.LogError( string.Format( "[LOG] : Build Error => [{0}]", cfg.m_Name ) );
            return false;
        }

		Debug.Log( string.Format( "[LOG] : Build Success => [{0}]", cfg.m_Name ) );
		return	true;
    }
	
    private static bool CopySDK( string defaultPluginPath, string specialPluginPath )
    {			
        try
        {
			string androidPluginPath = Application.dataPath + "/Plugins/Android";
            if ( ! Directory.Exists( androidPluginPath ) )
            {
                Debug.LogError("U3D android SDK Path Not Find!!!");
                return false;
            }

            CBuildIOHelp.DeleteDirectory( androidPluginPath );
			

			if( ! string.IsNullOrEmpty( defaultPluginPath ) )
			{
            	CBuildIOHelp.CopyDirectory( defaultPluginPath, androidPluginPath );
			}
			if( ! string.IsNullOrEmpty( specialPluginPath ) )
			{
                Debug.Log("coby path :" + androidPluginPath);
            	CBuildIOHelp.CopyDirectory( specialPluginPath, androidPluginPath );
			}

           // FileUtil.CopyFileOrDirectory( sdkBaskDKPath, androidPluginPath );
        }
        catch ( Exception ex )
        {
            Debug.LogError(ex.StackTrace);
            return false;
        }

        return true;
    }
	
    private static bool BuildAPK( string dir, string fileName )
    {
        try
        {
            string targetFullPath = string.Format( "{0}/{1}.apk", dir, fileName );

            if ( File.Exists( targetFullPath ) )
            {
                File.Delete( targetFullPath );
            }

            AssetDatabase.Refresh();

            List<string> EditorScenes = new List<string>();
            foreach ( EditorBuildSettingsScene scene in EditorBuildSettings.scenes )
            {
                if ( scene.enabled )
				{
                	EditorScenes.Add( scene.path );
				}
            }

            string res = BuildPipeline.BuildPlayer( EditorScenes.ToArray(), targetFullPath, BuildTarget.Android, BuildOptions.None );
            if ( res.Length > 0 )
            {
                Debug.LogError( "Build Failed => " + res );
                return false;
            }
        }
        catch ( Exception ex )
        {
            Debug.LogError(ex.StackTrace);
            return false;
        }

        return true;
    }
	
}


//#endif
