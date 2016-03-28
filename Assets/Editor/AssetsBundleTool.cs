using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Xml;
using Mono.Data.Sqlite;
/// <summary>
/// unity5版本资源assetsbundle打包工具[强制更新的资源打包]
/// author lgr
/// </summary>
public class AssetsBundleTool : EditorWindow {
    const string notification = "1,选择资源包发布的平台.2,选择资源包打包的输出目录.3,开始打包.";
    static string m_RootDirectory = "";
    static string m_RootUnforce = "";
    static string mVersion = "";
    static string mLog = "";
    static TargetPlatform m_TargetPlatform = TargetPlatform.StandaloneWindow;
    static string m_Force = "Assets/Resources/force";
    static string m_UnForce = "Assets/Resources/unforce";
    static string m_UnForceXML = "";
    static string XMLName = "";
    static string m_TagFile = "";
    static AssetsBundleTool window;
    static AssetBundleManifest mAssetBundleManifest;

    static CDBInstance mDataBaseInstance = null;
    public enum TargetPlatform : int
    {
        StandaloneWindow = (int)BuildTarget.StandaloneWindows64,
        StandaloneOSX = (int)BuildTarget.StandaloneOSXUniversal,
        Android = (int)BuildTarget.Android,
        IOS = (int)BuildTarget.iOS,
        WebPlayer = (int)BuildTarget.WebPlayer,
    }
    [MenuItem("GameEditor/BuildAssetBundle")]
    static void CallBuildWindow()
    {
        window = EditorWindow.GetWindow<AssetsBundleTool>();
        window.maxSize = new Vector2(605f, 1000f);
        window.minSize = new Vector2(605f, 200f);
        window.title = "AB Tool";
        m_TagFile = Application.dataPath + "/Resources/force/keytag";
        
        window.autoRepaintOnSceneChange = true;

        if (string.IsNullOrEmpty(m_RootDirectory))
        {
            m_RootDirectory = Application.dataPath + "/../../DragonAssetBundle/" +
                    m_TargetPlatform.ToString();//强制资源的输出路径
        }
        m_RootUnforce = Application.dataPath + "/../../DragonAssetBundle/unforce/" +
                    m_TargetPlatform.ToString();//非强制资源的输出路径

        m_TargetPlatform = TargetPlatform.StandaloneWindow;
        XMLName = "UnforceStandaloneWindowXML.xml";

    }
    void OnGUI() {
        
        EditorGUILayout.HelpBox(notification, MessageType.Info, true);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Target Platform:", GUILayout.Width(120f), GUILayout.Height(25));
        GUILayout.Space(10f);
        m_TargetPlatform = (TargetPlatform)EditorGUILayout.EnumPopup(m_TargetPlatform, GUILayout.Width(120f), GUILayout.Height(25));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("Version:", GUILayout.Width(80f), GUILayout.Height(20));
        GUILayout.Space(10f);
        mVersion = EditorGUILayout.TextField(mVersion, GUILayout.Width(80f), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10f);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("OutPut Directory *", GUILayout.Width(120f), GUILayout.Height(20));
        m_RootDirectory = EditorGUILayout.TextField(m_RootDirectory, GUILayout.Width(300f), GUILayout.Height(20));
        GUI.SetNextControlName("Browse");
        if (GUILayout.Button("Browse", GUILayout.Width(60f), GUILayout.Height(20)))
        {
            string path = EditorUtility.OpenFolderPanel("Browse", m_RootDirectory, "");
            m_RootDirectory = path.Length > 0 ? path : m_RootDirectory;
            GUI.FocusControl("Browse");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10f);
        if (GUILayout.Button("ForceBuild", GUILayout.Width(120f), GUILayout.Height(25)))
        {
            ForceBuild();
        }
        GUILayout.Space(10f);
        /*
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("DB Directory *", GUILayout.Width(120f), GUILayout.Height(20));
        m_UnForceDB = EditorGUILayout.TextField(m_UnForceDB, GUILayout.Width(250f), GUILayout.Height(20));
        GUI.SetNextControlName("BrowseDB");
        if (GUILayout.Button("BrowseDB", GUILayout.Width(90f), GUILayout.Height(20)))
        {
            string path = EditorUtility.OpenFilePanel("BrowseDB", m_UnForceDB, "");
            m_UnForceDB = path.Length > 0 ? path : m_UnForceDB;
            GUI.FocusControl("BrowseDB");
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10f);
         * */
        if (GUILayout.Button("UnForceBuild", GUILayout.Width(150f), GUILayout.Height(25)))
        {
            UnForceBuild();
        }


        if (GUILayout.Button("SetForceABName", GUILayout.Width(150f), GUILayout.Height(25)))
        {
            SetForceABName();
        }
        if (GUILayout.Button("SetUnForceABName", GUILayout.Width(150f), GUILayout.Height(25)))
        {
            SetUnForceABName();
        }

        if (GUILayout.Button("SetFirstABName", GUILayout.Width(150f), GUILayout.Height(25)))
        {
            SetFirstABName();
        }

        if (GUILayout.Button("ClearABName", GUILayout.Width(150f), GUILayout.Height(25)))
        {
            ClearABName();
        }

        GUILayout.Space(10f);
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("OutPut Log:", GUILayout.Width(120f), GUILayout.Height(25));
        GUILayout.Space(10f);
        EditorGUILayout.LabelField(mLog, GUILayout.Width(300f), GUILayout.Height(60));
        
    }

    static void ClearABName() {
        ClearABNameByPath(m_Force);
        ClearABNameByPath(m_UnForce);
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }
    static void ClearABNameByPath(string path)
    {
        List<string> fileList = new List<string>();
        GetAllFiles(path, fileList);

        string[] deps = AssetDatabase.GetDependencies(fileList.ToArray());
        foreach (string dp in deps)
        {
            if (fileList.Contains(dp) || dp.Contains(".cs") || dp.Contains(".shader") || dp.Contains(".meta") || dp.Contains(".xml")) continue;
            AssetImporter ai = AssetImporter.GetAtPath(dp);
            ai.assetBundleName = "";
        }
        for (int i = 0; i < fileList.Count; i++)
        {
            if (fileList[i].Contains(".cs") || fileList[i].Contains(".shader") || fileList[i].Contains(".meta")) continue;
            AssetImporter ai = AssetImporter.GetAtPath(fileList[i]);
            ai.assetBundleName = "";
        }
        EditorUtility.DisplayDialog("Success", "clear anname success!", "Yes");
    }

    static void SetFirstABName() {
        List<string> fileList = new List<string>();
        #region xml
        fileList.Add("force/config/hero_conf.xml");
        fileList.Add("force/config/occ_conf.xml");
        fileList.Add("force/config/sign_conf.xml");
        fileList.Add("force/config/shop_conf.xml");
        fileList.Add("force/config/npc_conf.xml");
        fileList.Add("force/config/mission_conf.xml");
        fileList.Add("force/config/smission_conf.xml");
        fileList.Add("force/config/skill_conf.xml");
        fileList.Add("force/config/ai_conf.xml");
        fileList.Add("force/config/lottery_conf.xml");
        fileList.Add("force/config/item_conf.xml");
        fileList.Add("force/config/compose_conf.xml");
        fileList.Add("force/config/task_conf.xml");
        fileList.Add("force/config/game_conf.xml");
        fileList.Add("force/config/TextConfig.xml");
        fileList.Add("force/config/money_conf.xml");
        fileList.Add("force/config/story_conf.xml");
        fileList.Add("force/config/vip_conf.xml");
        fileList.Add("force/config/charge_conf.xml");
        fileList.Add("force/config/firstfight_conf.xml");
        fileList.Add("force/config/userguide_conf.xml");
        #endregion
        #region effect
        fileList.Add("force/effect/baozha.prefab");
        fileList.Add("force/effect/baozhaboss.prefab");
        fileList.Add("force/effect/beattack.prefab");
        fileList.Add("force/effect/blue.prefab");
        fileList.Add("force/effect/buff_1001.prefab");
        fileList.Add("force/effect/chusheng.prefab");
        fileList.Add("force/effect/fuhuo.prefab");
        fileList.Add("force/effect/qiehuan.prefab");
        fileList.Add("force/effect/red.prefab");
        fileList.Add("force/effect/rolearrow.prefab");
        fileList.Add("force/effect/yan.prefab");
        fileList.Add("force/effect/skill11.prefab");
        fileList.Add("force/effect/skill09.prefab");
        fileList.Add("force/effect/skill01.prefab");
        fileList.Add("force/effect/superskill.prefab");
        fileList.Add("force/effect/dan01.prefab");
        fileList.Add("force/effect/skill08.prefab");
        fileList.Add("force/effect/skill07.prefab");
        #endregion
        #region sound 
        fileList.Add("force/sound/skill0072.prefab");
        fileList.Add("force/sound/bhrt.prefab");
        fileList.Add("force/sound/skill0082.prefab");
        fileList.Add("force/sound/cslsp.prefab");
        fileList.Add("force/sound/skill0012.prefab");
        fileList.Add("force/sound/ylpf.prefab");
        fileList.Add("force/sound/heti.prefab");
        fileList.Add("force/sound/skill0011.prefab");
        fileList.Add("force/sound/attack0011.prefab");
        fileList.Add("force/sound/attack0012.prefab");
        fileList.Add("force/sound/attack0013.prefab");
        fileList.Add("force/sound/beattack11.prefab");
        fileList.Add("force/sound/bossdie.prefab");
        fileList.Add("force/sound/fightbg.prefab");
        fileList.Add("force/sound/fightbgboss.prefab");
        fileList.Add("force/sound/loginbg.prefab");
        fileList.Add("force/sound/lose.prefab");
        fileList.Add("force/sound/walk.prefab");
        fileList.Add("force/sound/win.prefab");
        fileList.Add("force/sound/boom.prefab");
        #endregion
        #region scene
        fileList.Add("force/scene/scene03.prefab");
        #endregion
        #region model
        fileList.Add("force/model/hint.prefab");
        fileList.Add("force/model/hintboss.prefab");
        fileList.Add("force/model/hp.prefab");
        fileList.Add("force/model/card/3000001.prefab");
        fileList.Add("force/model/card/3000007.prefab");
        fileList.Add("force/model/card/3000008.prefab");
        fileList.Add("force/model/card/4000000.prefab");
        fileList.Add("force/model/npc/1000001.prefab");
        fileList.Add("force/model/npc/2000003.prefab");
        fileList.Add("force/model/npc/2000004.prefab");
        fileList.Add("force/model/npc/2000005.prefab");
        #endregion
        #region ui
        fileList.Add("force/ui/battlelabel/numlabel.prefab");
        fileList.Add("force/ui/diamondshopui/notdiamondshopui.prefab");
        fileList.Add("force/ui/fightherosui/bossui.prefab");
        fileList.Add("force/ui/fightherosui/fightawardui.prefab");
        fileList.Add("force/ui/fightherosui/fightskillui.prefab");
        fileList.Add("force/ui/fightherosui/fightui.prefab");
        fileList.Add("force/ui/fightherosui/pauseui.prefab");
        fileList.Add("force/ui/fightherosui/reliveui.prefab");
        fileList.Add("force/ui/loginui/loginui.prefab");
        fileList.Add("force/ui/msgboxui/hintui.prefab");
        fileList.Add("force/ui/msgboxui/msgboxui.prefab");
        fileList.Add("force/ui/progressbarui/progressbarui.prefab");
        fileList.Add("force/ui/settlementui/settlementui.prefab");
        fileList.Add("force/ui/storyui/storyui.prefab");
        fileList.Add("force/ui/userguide/userguideui.prefab");
        #endregion
        for (int i = 0; i < fileList.Count; i++)
        {
            if (fileList[i].Contains(".cs") || fileList[i].Contains(".shader") || fileList[i].Contains(".meta")) continue;

            AssetImporter ai = AssetImporter.GetAtPath("Assets/Resources/" + fileList[i]);
            if (ai == null) {
                Debug.LogError(fileList[i]);
            }
            if (fileList[i].Contains(".xml"))
            { //配置文件
                ai.assetBundleName = "force/config/xmldata";
            }
            else
            {
                string path = "Assets/Resources/"+fileList[i];
                int index1 = 16;
                int index2 = path.LastIndexOf(".");
                string abName = path.Substring(index1 + 1, index2 - index1 - 1);
                ai.assetBundleName = abName;
            }

        }
        EditorUtility.DisplayDialog("Success", "set first abname success!", "Yes");
    }

    static void SetForceABName() {
        List<string> fileList = new List<string>();
        GetAllFiles(m_Force, fileList);

        for (int i = 0; i < fileList.Count; i++) {
            if (fileList[i].Contains(".cs") || fileList[i].Contains(".shader") || fileList[i].Contains(".meta")) continue;
            AssetImporter ai = AssetImporter.GetAtPath(fileList[i]);
            if (fileList[i].Contains(".xml"))
            { //配置文件
                ai.assetBundleName = "force/config/xmldata";
            }
            else {
                string path = fileList[i];
                int index1 = 16;
                int index2 = path.LastIndexOf(".");
                string abName = path.Substring(index1 + 1, index2 - index1 - 1);
                ai.assetBundleName = abName; 
            }

        }
        EditorUtility.DisplayDialog("Success", "set abname success!", "Yes");
    }


    static void SetUnForceABName()
    {
        List<string> fileList = new List<string>();
        GetAllFiles(m_UnForce, fileList);

        for (int i = 0; i < fileList.Count; i++)
        {
            if (fileList[i].Contains(".cs") || fileList[i].Contains(".shader") || fileList[i].Contains(".meta")) continue;
            AssetImporter ai = AssetImporter.GetAtPath(fileList[i]);
            string path = fileList[i];
            int index1 = 16;
            int index2 = path.LastIndexOf(".");
            string abName = path.Substring(index1 + 1, index2 - index1 - 1);
            ai.assetBundleName = abName;

        }
        EditorUtility.DisplayDialog("Success", "set anname success!", "Yes");
    }

    /// <summary>
    /// 打包
    /// </summary>
    static void ForceBuild()
    {
        if (!File.Exists(m_TagFile))
        {
            //EditorUtility.DisplayDialog("Error", "dll had not encrypted!", "Yes");
            //return;
        }
        if (m_RootDirectory == "" || m_RootDirectory.Length <= 0) {
            EditorUtility.DisplayDialog("Error", "must browse a output directory first!", "Yes");
            mLog = "error:must browse a output directory first!";
            return;
        }
        if (mVersion == "" || mVersion.Length <= 0) {
            EditorUtility.DisplayDialog("Error", "must set a assets version example 1.0.0!", "Yes");
            mLog = "error:must set a assets version example 1.0.0";
            return; 
        }
        if (!CheckVersionFormat()) {
            return;
        }
        ClearAll();
        if (!Directory.Exists(m_RootDirectory)) {
            Directory.CreateDirectory(m_RootDirectory);
        }
        
        List<string> abFiles = new List<string>();
        List<string> fileList = new List<string>();
        GetAllFiles(m_Force, fileList);
        List<string> allDeps = new List<string>();//存放被依赖的资源
        /* update 20151112
        string[] deps = AssetDatabase.GetDependencies(fileList.ToArray());
        
        foreach(string dp in deps)
        {
            if (abFiles.Contains(dp))
            {
                Debug.LogError("con:  "+dp);
                continue;
                
            }
            //if (fileList.Contains(dp) || dp.Contains(".cs") || dp.Contains(".shader") || dp.Contains(".meta")) continue;
            if (fileList.Contains(dp) || dp.Contains(".cs") || dp.Contains(".meta")) continue;
            if (allDeps.Contains(dp))
            { //和其他的有公共依赖,设置abName为GUID
                AssetImporter ai = AssetImporter.GetAtPath(dp);
                ai.assetBundleName = AssetDatabase.AssetPathToGUID(dp);
                
            }
            else {
                allDeps.Add(dp);
            }
        }
        */
        for (int i = 0; i < fileList.Count; i++) {
            string[] fileArr = new string[] { fileList[i] };
            string[] deps = AssetDatabase.GetDependencies(fileArr);
            foreach (string dp in deps)
            {
                if (abFiles.Contains(dp))
                {
                    Debug.LogError("con:  " + dp);
                    continue;

                }
                //if (fileList.Contains(dp) || dp.Contains(".cs") || dp.Contains(".shader") || dp.Contains(".meta")) continue;
                
                if (fileList.Contains(dp) || dp.Contains(".cs") || dp.Contains(".meta")) continue;
                if (allDeps.Contains(dp))
                { //和其他的有公共依赖,设置abName为GUID
                    AssetImporter ai = AssetImporter.GetAtPath(dp);
                    ai.assetBundleName = AssetDatabase.AssetPathToGUID(dp);

                }
                else
                {
                    allDeps.Add(dp);
                }
            }
        }

        mAssetBundleManifest = BuildPipeline.BuildAssetBundles(m_RootDirectory, BuildAssetBundleOptions.ForceRebuildAssetBundle, (BuildTarget)((int)m_TargetPlatform));

        if (mAssetBundleManifest != null)
        {
            string[] arr = mAssetBundleManifest.GetAllAssetBundles();
            for (int i = 0; i < arr.Length; i++)
            {
                //Debug.LogError("successed:"+arr[i]);
            }
            mLog = "successed:" + arr.Length;
            ReNameMD5Files(arr);
        }
        AssetDatabase.Refresh();
        
        if (File.Exists(m_TagFile))
        {
            File.Delete(m_TagFile);
        }
        EditorUtility.DisplayDialog("Success", "assetbundle build success!", "Yes");
    }

    static void ClearAll() {
        if (Directory.Exists(m_RootDirectory))
        {
            //Directory.Exists(m_RootDirectory);
            Directory.Delete(m_RootDirectory,true);
        }
    }
    /// <summary>
    /// 以md5标识命名资源包
    /// </summary>
    static void ReNameMD5Files(string[] strArr) {
        List<string> arr = new List<string>();
        for (int i = 0; i < strArr.Length; i++) {
            arr.Add(strArr[i]);
            int index = strArr[i].LastIndexOf("/");
            string path = "";
            if (index != -1) {
                path = strArr[i].Substring(0, strArr[i].Length - index);
            }
            string newName = strArr[i] + ".manifest";
            if (path != "") {
                newName = path + "/" + newName;
            }
            arr.Add(strArr[i] + ".manifest");
        }
        if (m_TargetPlatform == TargetPlatform.StandaloneWindow) {
            arr.Add("StandaloneWindow");
            arr.Add("StandaloneWindow.manifest");
        }
        else if (m_TargetPlatform == TargetPlatform.IOS)
        {
            arr.Add("IOS");
            arr.Add("IOS.manifest");
        }
        else if (m_TargetPlatform == TargetPlatform.Android)
        {
            arr.Add("Android");
            arr.Add("Android.manifest");
        }

        string xmlStr = "<root version='" + mVersion + "'>";
        for (int i = 0; i < arr.Count; i++) {
            byte[] bytes = File.ReadAllBytes(m_RootDirectory + "/" + arr[i]);
            string hash = GetMd5(bytes);
            string name = arr[i];
            string realName = name;
            int index = name.LastIndexOf("/");
            string path="";
            if (index != -1)
            {
                path=name.Substring(0,index);
            }
            string newPath = m_RootDirectory + "/" + hash;
            if (path != "")
            {
                newPath = m_RootDirectory + "/" + path + "/" + hash;
            }

            if (arr[i] == "StandaloneWindow.manifest" || arr[i] == "IOS.manifest" || arr[i] == "Android.manifest")
            {
                xmlStr += "<asset name='" + realName + "' md5='" + hash + "' size='" + bytes.Length + "' path='" + path + "'/>";
                File.WriteAllBytes(newPath, bytes);
            }
            else {
                if (!arr[i].Contains(".manifest")) {
                    xmlStr += "<asset name='" + realName + "' md5='" + hash + "' size='" + bytes.Length + "' path='" + path + "'/>";
                    File.WriteAllBytes(newPath, bytes);
                }
            }
            //Debug.LogError(m_RootDirectory + "/" + arr[i]);
            File.Delete(m_RootDirectory + "/" + arr[i]);
        }
        xmlStr += "</root>";
        SaveVersionXML(xmlStr);
    }
    /// <summary>
    /// 保存版本配置文件
    /// </summary>
    /// <param name="xmlStr"></param>
    static void SaveVersionXML(string xmlStr)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlStr);
        xml.Save(m_RootDirectory + "/ResVersion.xml");
    }
    /// <summary>
    /// 获取字节流文件的Md5码
    /// </summary>
    /// <param name="bytes">字节流</param>
    /// <returns></returns>
    static string GetMd5(byte[] bytes)
    {
        
        MD5 md5 = MD5.Create();
        byte[] mds = md5.ComputeHash(bytes);
        string md5Str = "";
        for (int i = 0; i < mds.Length; i++)
        {
            md5Str = md5Str + mds[i].ToString("X");
        }

        return md5Str;
    }
    /// <summary>
    /// 简单检查version格式
    /// </summary>
    /// <returns></returns>
    static bool CheckVersionFormat() {
        bool b = true;
        try
        {
            System.Version version = new System.Version(mVersion);
            Debug.LogError(version);
        }
        catch(Exception e) {
            Debug.LogError(e.ToString());
            mLog = "error:must set a assets version example 1.0.0";
            b = false;
        }
        
        return b;
    }

    ///////////////////////////////////////////unforce目录下的非强制更新的资源打包[例如物品icon资源等]///////////////////////////////////
    static void UnForceBuild() {
        /*
        AudioClip obj = Resources.Load("unforce/audios/10008", typeof(AudioClip)) as AudioClip;
        GameObject go = new GameObject();
        go.AddComponent<AudioSource>().clip = obj;
        go.GetComponent<AudioSource>().Play();
        return;
         * */
        Debug.LogError(m_TargetPlatform);
        m_RootUnforce = Application.dataPath + "/../../DragonAssetBundle/unforce/" +
                    m_TargetPlatform.ToString();//非强制资源的输出路径
        if(m_TargetPlatform == TargetPlatform.StandaloneWindow){
            XMLName = "UnforceStandaloneWindowXML.xml";
        }else if(m_TargetPlatform == TargetPlatform.Android){
            XMLName = "UnforceAndroidXML.xml";
        }else if(m_TargetPlatform == TargetPlatform.IOS){
            XMLName = "UnforceIOSXML.xml";
        }

        m_UnForceXML = "E:/WorkSpace/AssetBundle/unforce/" + XMLName;

        if (m_UnForceXML == "") {
            EditorUtility.DisplayDialog("Error", "must select a unforceDB path!", "Yes");
            mLog = "error:must select a unforceDB path";
            return; 
        }
        if (Directory.Exists(m_RootUnforce))
        {
            //Directory.Exists(m_RootUnforce);
            Directory.Delete(m_RootUnforce, true);
        }
        if (!Directory.Exists(m_RootUnforce))
        {
            Directory.CreateDirectory(m_RootUnforce);
        }
        List<string> paths = new List<string>();//非强制更新的资源文件夹下的所有文件目录
        GetAllFiles(m_UnForce,paths);
        AssetBundleBuild[] abbs =new AssetBundleBuild[paths.Count];
        List<string> abNames = new List<string>();
        //设置abName
        for (int i = 0; i < paths.Count; i++)
        {
            string name = paths[i].Substring(17, paths[i].Length - 17);
            
            int dotIndex = name.LastIndexOf(".");
            string abName = name.Substring(0, dotIndex);
            //Debug.LogError(abName);
            AssetImporter ai = AssetImporter.GetAtPath(paths[i]);
           
            ai.assetBundleName = abName;

            int lastIndex = abName.LastIndexOf("/") + 1;
            string assetName = abName.Substring(lastIndex, abName.Length - lastIndex);

            abbs[i] = new AssetBundleBuild();
            abbs[i].assetBundleName = abName;
            abbs[i].assetNames = new string[1] { paths[i] };
            
            abNames.Add(abName);
            
        }

        BuildPipeline.BuildAssetBundles(m_RootUnforce, abbs, BuildAssetBundleOptions.ForceRebuildAssetBundle, (BuildTarget)((int)m_TargetPlatform));
        AssetDatabase.Refresh();
        
        List<string> files = new List<string>();
        GetAllFiles(m_RootUnforce, files);
        //删除menifest
        for (int i = 0; i < files.Count; i++) {
            
            if (files[i].Contains(".manifest") || files[i].Contains("StandaloneWindow/StandaloneWindow") || files[i].Contains("IOS/IOS") || files[i].Contains("Android/Android"))
            {
                File.Delete(files[i]);
            }
        }

        //打包完成还原abName
        for (int i = 0; i < paths.Count; i++)
        {
            AssetImporter ai = AssetImporter.GetAtPath(paths[i]);
            ai.assetBundleName = "";
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();

        //版本写入数据库文件
        #region old
        /*
        if (!File.Exists(m_UnForceDB))
        {
            FileStream fs = File.Create(m_UnForceDB);
            fs.Close();
            fs.Dispose();
            mDataBaseInstance = CDataBaseManager.GetDBInstance(m_UnForceDB);
            if (mDataBaseInstance.BeginTransaction())
            {
                mDataBaseInstance.CreateTable("unforceTable", 
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
                mDataBaseInstance.EndTransaction();
            }
            //mDataBaseInstance.Close();
        }
        DBVersionManager.ToolInit(m_UnForceDB);
        DBVersionManager.ToolOpenDB();
        
        for (int i = 0; i < abNames.Count; i++) {
            DataBaseInfo result = DBVersionManager.GetDataInfo(DBVersionManager.GetDBInstance(),abNames[i]);
            if (result == null)
            { //不存在 写入
                byte[] bytes = File.ReadAllBytes(m_RootUnforce + "/" + abNames[i]);
                string md5 = GetMd5(bytes);
                int size = bytes.Length;
                int romote = 1;
                DBVersionManager.InsertInfo(DBVersionManager.GetDBInstance(), "unforceTable", new string[] {
						"'"+abNames[i]+"'",
						"'"+md5+"'",
						romote.ToString(),
						size.ToString() });
            }
            else { //存在，比较2者的md5是否一致
                byte[] bytes = File.ReadAllBytes(m_RootUnforce + "/" + abNames[i]);
                string md5 = GetMd5(bytes);
                int size = bytes.Length;
                if (result.md5 == md5)
                {
                    continue;
                }
                else { //出现了不同，修改数据库
                    int romote = 1;
                    DBVersionManager.UpdateInfo(DBVersionManager.GetDBInstance(), "unforceTable",
                        new string[] {
						"md5",
						"romote",
						"size" }, 
                        new string[] {
						"'"+md5+"'",
						romote.ToString(),
						size.ToString() }, "path", "'" + abNames[i] + "'");
                }
            }
        }
         */
        #endregion
        string xmlStr = "<root>";
        for (int i = 0; i < abNames.Count; i++)
        {
            byte[] bytes = File.ReadAllBytes(m_RootUnforce + "/" + abNames[i]);
            string md5 = GetMd5(bytes);
            int size = bytes.Length;
            xmlStr += "<asset romote='" + 1 + "' md5='" + md5 + "' size='" + bytes.Length + "' path='" + abNames[i] + "'/>";
        }
        xmlStr += "</root>";
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlStr);
        xml.Save(m_UnForceXML);
        //DBVersionManager.ToolCloseDB();
        EditorUtility.DisplayDialog("Success", "assetbundle build success!", "Yes");
    }

    static void GetAllFiles(string directory,List<string> paths) {
        string[] files = Directory.GetFiles(directory);
        string[] directorys = Directory.GetDirectories(directory);

        for (int i = 0; i < files.Length; i++) {
            string[] arr = files[i].Split('\\');
            string path = arr[0] + "/" + arr[1];
            //Debug.LogError(path);
            if (path.Contains(".meta")) continue;
            paths.Add(path);
        }
        for (int j = 0; j < directorys.Length; j++) {
            string[] arr = directorys[j].Split('\\');
            string path = arr[0] + "/" + arr[1];
            //Debug.LogError(path);
            GetAllFiles(path, paths);
        }
    }
}
