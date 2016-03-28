using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
/// <summary>
/// ab资源管理器
/// author lgr
/// </summary>

public class AssetsBundleManager : IResManager {
    //public static AssetsBundleManager Instance;
    /// <summary>
    /// 本地资源根目录路径
    /// </summary>
    private string mLocalResRootPath = null;

    private VoidDelegate OnAssetsPrepareComplete = null;
    private StringDelegate OnAssetsLoadComplete = null;
    /// <summary>
    /// 流媒体资源路径
    /// </summary>
    private string mStreamResRootPath = null;
    private string mMainABPath = null;

    private Dictionary<string, VersionAssetData> mNameMD5Dic = null;//根据资源名，得到md5码和路径
    //private Dictionary<string, AssetsBundleData> mAssetsDpDic = null;//资源依赖记录集合
    private Dictionary<string, AssetsBundleData> mAssetsABDic = null;//ab资源记录集合
    //private Dictionary<string, List<AssetsLoadedData>> mAssetsLoaded = null;//已经加载的资源集合,包括被依赖的资源
    private string RVL = "ResVersion.xml";
    private int mIndex = 0;//版本xml尝试加载次数
    class LoadTask {
        public string callBackEvent;
        public List<string> list;
    }
    private List<LoadTask> mDownLoadQueue = null;//加载任务队列
	void Awake () {
        if (!IResManager.ResDebug)
        {
            IResManager.Instance = this;
        }
        //DontDestroyOnLoad(gameObject);
        mLocalResRootPath =Application.persistentDataPath;
        mStreamResRootPath = "File:///" + Application.dataPath + "/StreamingAssets";
        mMainABPath = "StandaloneWindow";
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        //mLocalResRootPath =  Application.dataPath + "/../../AssetBundle";
        mLocalResRootPath =  Application.dataPath + "/../../AssetBundle/Local";
        mStreamResRootPath = "File:///" + Application.dataPath + "/StreamingAssets";
        mMainABPath = "StandaloneWindow";
#elif UNITY_IPHONE
        mLocalResRootPath =  Application.persistentDataPath;
        mStreamResRootPath = "File:///" + Application.dataPath + "/Raw";
        mMainABPath = "IOS";
#elif UNITY_ANDROID
        mLocalResRootPath = Application.persistentDataPath;
        mStreamResRootPath = "jar:File:///" + Application.dataPath + "!/assets";
        mMainABPath = "Android";
#endif
        //mAssetsDpDic = new Dictionary<string, AssetsBundleData>();//被依赖
        //mAssetsLoaded = new Dictionary<string, List<AssetsLoadedData>>();//加载的主资源
        mAssetsABDic = new Dictionary<string, AssetsBundleData>();
        mDownLoadQueue = new List<LoadTask>();//加载队列

        StartCoroutine(Run());
	}
    /// <summary>
    /// 准备 用于资源更新结束之后调用
    /// </summary>
    public override void Prepare(VoidDelegate prepareCompleteCallBack,StringDelegate loadCompleteCallBack) {
        OnAssetsLoadComplete = loadCompleteCallBack;
        OnAssetsPrepareComplete = prepareCompleteCallBack;
        StartCoroutine(SetMD5AssetsList());
    }
    /// <summary>
    /// 取资源的md5信息
    /// </summary>
    /// <returns></returns>
    IEnumerator SetMD5AssetsList() {
        mNameMD5Dic = new Dictionary<string, VersionAssetData>();
        string localPath = mLocalResRootPath + "/" + RVL;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        localPath = mLocalResRootPath + "/" + RVL;
#elif UNITY_ANDROID
        localPath = mLocalResRootPath + "/" + RVL;
#elif UNITY_IPHONE
        localPath = mLocalResRootPath + "/" + RVL;
#endif
        yield return null;
        WWW localWWW = new WWW("File://" + localPath);
        yield return localWWW;
        if (localWWW.error != null)
        {
            if (mIndex > 5)
            {
                Debug.LogError("can not find:" + localPath);
            }
            else
            {
                mIndex += 1;
                Debug.LogError("test www load:" + mIndex);
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(SetMD5AssetsList());
            }
        }
        else {
            mIndex = 0;
            XmlDocument document = new XmlDocument();
            document.LoadXml(localWWW.text);
            XmlNode root = document.SelectSingleNode("root");

            foreach (XmlNode _node in root.ChildNodes)
            {
                XmlElement node = _node as XmlElement;
                if (node == null) { continue; }
                if (node.Name == "asset")
                {
                    VersionAssetData loadAsset = new VersionAssetData();
                    loadAsset.name = node.GetAttribute("name");
                    loadAsset.md5 = node.GetAttribute("md5");
                    loadAsset.size = System.Convert.ToInt32(node.GetAttribute("size"));
                    loadAsset.path = node.GetAttribute("path");
                    if (!mNameMD5Dic.ContainsKey(loadAsset.name))
                    {
                        mNameMD5Dic.Add(loadAsset.name, loadAsset);
                    }
                }
            }
            yield return StartCoroutine(WWWLoadMainAssets());
        }
    }
    /// <summary>
    /// 增加加载队列
    /// </summary>
    /// <param name="assets"></param>
    public override void AddResTask(List<string> assets, string callBackEvent)
    {
        LoadTask loadTask = new LoadTask();
        loadTask.list = assets;
        loadTask.callBackEvent = callBackEvent;
        if(!mDownLoadQueue.Contains(loadTask)){
            mDownLoadQueue.Add(loadTask);
        } 
    }
    
    IEnumerator Run()
    {
        while (true)
        {
            if (mDownLoadQueue.Count > 0)
            {
                LoadTask task = mDownLoadQueue[0];
                mDownLoadQueue.RemoveAt(0);

                yield return StartCoroutine(LoadAssets(task.list));
                if (OnAssetsLoadComplete != null)
                {
                    Debug.LogError("load complete...:" + task.callBackEvent);
                    OnAssetsLoadComplete(task.callBackEvent);
                    //OnAssetsLoadComplete = null;
                }
                
            }

            yield return null;
        }
    }
    /// <summary>
    /// 加载一个队列
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    IEnumerator LoadAssets(List<string> list) {
        mTotalNum = 0;
        mLoadedNum = 0;
        for(int i=0;i<mDownLoadQueue.Count;i++){
            mTotalNum+=mDownLoadQueue[i].list.Count;
        }
        mTotalNum += list.Count;

        for (int j = 0; j < list.Count; j++) {
            yield return StartCoroutine(WWWLoadAssets(list[j], false));
        }
        yield return null;
    }
    private AssetBundleManifest mMainManifest = null;
    /// <summary>
    /// 加载各个平台的总记录的ab
    /// </summary>
    /// <returns></returns>
    IEnumerator WWWLoadMainAssets() {
        string url = GetMainABPath();

        VersionAssetData vad = mNameMD5Dic[mMainABPath];

        yield return null;
        WWW mainWWW = new WWW(url);
        yield return mainWWW;
        if (mainWWW.error != null)
        {
            Debug.LogError("WWWLoadMainAssets Error:" + url);
        }
        else {
            AssetBundle ab = mainWWW.assetBundle;
            mMainManifest = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
            ab.Unload(false);
            //开始加载任务队列
            //yield return StartCoroutine(Run());

            if (OnAssetsPrepareComplete != null)
            {
                OnAssetsPrepareComplete();
                OnAssetsPrepareComplete = null;
            }
            else {
                Debug.LogError("OnAssetsPrepareComplete is null");
            }
        }
    }
    /// <summary>
    /// 加载资源包
    /// </summary>
    /// <param name="dpName"></param>
    /// <param name="mainName">主资源名称</param>
    /// <returns></returns>
    IEnumerator WWWLoadAssets(string name,bool dp) {
        if (mAssetsABDic.ContainsKey(name))
        {
            AssetsBundleData abd = mAssetsABDic[name];
            abd.depIndex += 1;
        }
        else {
            if (mNameMD5Dic.ContainsKey(name))
            {
                VersionAssetData vad = mNameMD5Dic[name];
                /*
                string url = GetAssetsWWWPath(vad.path, vad.md5);
                WWW abWWW = new WWW(url);
                yield return abWWW;
                */
                string url = GetAssetsFilePath(vad.path, vad.md5);
                byte[] bytes = ComTool.ReadFile(url);

                if (bytes != null)
                {
                    AssetBundle ab = null;
                    
                    if (!dp)
                    { //作为主资源加载的
                        ab = AssetBundle.CreateFromMemoryImmediate(bytes);
                        string[] dpAbs = mMainManifest.GetAllDependencies(name);//取这个资源的所有依赖资源
                        if (dpAbs.Length == 0)//没有依赖
                        {
                            #region
                            AssetsBundleData abd = new AssetsBundleData();
                            abd.name = name;
                            abd.depIndex = 1;
                            abd.ab = ab;
                            mAssetsABDic.Add(name, abd);
                            #endregion
                        }
                        else
                        { //存在依赖资源
                            #region
                            
                            for (int i = 0; i < dpAbs.Length; i++)
                            {
                                if (mAssetsABDic.ContainsKey(dpAbs[i]))
                                { //依赖计数器中已经存在了 计数器+1
                                    AssetsBundleData abd = mAssetsABDic[dpAbs[i]];
                                    abd.depIndex += 1;
                                }
                                else
                                {
                                    yield return StartCoroutine(WWWLoadAssets(dpAbs[i], true));
                                }
                            }
                            #endregion
                            //依赖都加载完毕了 再看主资源
                            #region
                            if (!mAssetsABDic.ContainsKey(name))
                            {
                                AssetsBundleData abd = new AssetsBundleData();
                                abd.name = name;
                                abd.depIndex = 1;
                                abd.ab = ab;
                                mAssetsABDic.Add(name, abd);
                            }
                            else
                            {
                                AssetsBundleData abd = mAssetsABDic[name];
                                abd.depIndex += 1;
                            }
                            #endregion
                        }

                    }
                    else
                    {//作为被依赖资源下载
                        AssetsBundleData abd = null;
                        if (mAssetsABDic.ContainsKey(name))
                        {
                            abd = mAssetsABDic[name];
                            abd.depIndex += 1;
                        }
                        else
                        {
                            ab = AssetBundle.CreateFromMemoryImmediate(bytes);
                            abd = new AssetsBundleData();
                            abd.name = name;
                            abd.depIndex = 1;
                            abd.ab = ab;
                            mAssetsABDic.Add(name, abd);
                        }
                    }
                    
                    if (!dp)
                    { //资源包加载完成计数+1
                        mLoadedNum += 1;
                        //Debug.LogError("module loaded num:" + mLoadedNum);
                    }
                }
                else
                {
                    Debug.LogError("cannot find:" + url);
                }
            }
        }
        
    }
    /// <summary>
    /// 迭代unload资源加载的ab
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IEnumerator DisposeDPAB(string name) {
        string[] dpAbs = mMainManifest.GetAllDependencies(name);//取这个资源的所有依赖资源
        if (dpAbs.Length == 0)
        { //这个资源已经是最小单元了
            if (mAssetsABDic.ContainsKey(name))
            {
                AssetsBundleData abd = mAssetsABDic[name];
                
                abd.depIndex -= 1;
                if (abd.depIndex <= 0)
                {
                    if (abd.ab != null)
                    {
                        abd.ab.Unload(false);
                        abd.ab = null;
                    }
                    mAssetsABDic.Remove(name);
                    abd = null;
                }
            }
        }
        else {
            for (int i = 0; i < dpAbs.Length; i++)
            {
                yield return StartCoroutine(DisposeDPAB(dpAbs[i]));
            }
        }
        /*
        if (mAssetsDpDic.ContainsKey(name))
        {
            AssetsBundleData abd = mAssetsDpDic[name];
            abd.depIndex -= 1;
            if (abd.depIndex <= 0)
            {
                if (abd.ab != null)
                {
                    abd.ab.Unload(false);
                    abd.ab = null;
                }
                mAssetsDpDic.Remove(name);
                abd = null;
            }
        }
         * */
        yield return null;
    }
    /// <summary>
    /// 加载一个ab资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public override UnityEngine.Object GetRes(string abName, string assetName)
    {
        
        UnityEngine.Object obj = null;
        if (mAssetsABDic.ContainsKey(abName))
        {
            UnityEngine.Object[] objs = mAssetsABDic[abName].ab.LoadAllAssets();
            obj = null;
            for (int i = 0; i < objs.Length; i++) {
                if (objs[i].name == assetName) {
                    obj = objs[i];
                    break;
                }
            }

            if (obj == null)
            {
                Debug.LogError("module not contains:" + assetName);
            }
        }
        else {
            Debug.LogError("module not loaded:" + abName);
        }
        return obj;
    }

    public override bool HasRes(string path)
    {
        if (mAssetsABDic.ContainsKey(path))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 卸载所有已加载的资源包
    /// </summary>
    public override void UnLoadAllRes() {
        Debug.LogError("UnLoadAllRes");
        foreach (string key in mAssetsABDic.Keys)
        {
            AssetsBundleData abd = mAssetsABDic[key];
            if (abd.ab != null) {
                abd.ab.Unload(false);
                abd.ab = null;
                abd = null;
            }
            
        }
        mAssetsABDic.Clear();
        
        //mAssetsLoaded.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    /// <summary>
    /// 卸载指定的资源包
    /// </summary>
    /// <param name="list"></param>
    public override void UnLoadRes(List<string> list)
    {
        for (int i = 0; i < list.Count; i++) { 
            //卸载依赖资源
            UnLoadDpAssets(list[i]);//new

        }
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    public override void UnLoadRes(string str)
    {
        List<string> list = new List<string>();
        list.Add(str);
        UnLoadRes(list);
    }
    /// <summary>
    /// 清除被依赖的资源包
    /// </summary>
    /// <param name="name"></param>
    private void UnLoadDpAssets(string name) {
        string[] dpAbs = mMainManifest.GetAllDependencies(name);//取这个依赖资源的所有依赖资源
        if (dpAbs.Length == 0)
        { //这个资源包没有依赖其他的
            if (mAssetsABDic.ContainsKey(name))
            {
                //计数器-1
                AssetsBundleData abd = mAssetsABDic[name];
                abd.depIndex -= 1;
                if (abd.depIndex <= 0)
                {
                    if (abd.ab != null)
                    {
                        abd.ab.Unload(false);
                        abd.ab = null;
                    }
                    Debug.LogError("remove ab:" + name);
                    mAssetsABDic.Remove(name);//移除索引计数器
                    abd = null;
                }
            }
        }
        else { //还有依赖的
            for (int i = 0; i < dpAbs.Length; i++) {
                UnLoadDpAssets(dpAbs[i]);
            }
        }
        if (mAssetsABDic.ContainsKey(name))
        {
            //计数器-1
            AssetsBundleData abd = mAssetsABDic[name];
            abd.depIndex -= 1;
            if (abd.depIndex <= 0)
            {
                if (abd.ab != null)
                {
                    abd.ab.Unload(false);
                    abd.ab = null;
                }
                Debug.LogError("remove ab:" + name);
                mAssetsABDic.Remove(name);//移除索引计数器
                abd = null;
            }
        }
    }
    /// <summary>
    /// 获取路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="md5"></param>
    /// <returns></returns>
    private string GetAssetsWWWPath(string path,string md5) {
        string url = "";
        if (path == "")
        {
            url = mLocalResRootPath + "/" + md5;
        }
        else {
            url = mLocalResRootPath + "/" + path + "/" + md5;
        }
        if (File.Exists(url))
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            url = "File://" + url;
#elif UNITY_ANDROID
            url = "File://" + url;
#elif UNITY_IPHONE
            url = "File://" + url;
#endif
            return url;
        }
        else {
            if (path == "")
            {
                url = mStreamResRootPath + "/" + md5;
            }
            else
            {
                url = mStreamResRootPath + "/" + path + "/" + md5;
            }
            return url;
        }
           
    }
    private string GetAssetsFilePath(string path, string md5)
    {
        string url = "";
        if (path == "")
        {
            url = mLocalResRootPath + "/" + md5;
        }
        else
        {
            url = mLocalResRootPath + "/" + path + "/" + md5;
        }
        if (File.Exists(url))
        {
            return url;
        }
        else
        {
            if (path == "")
            {
                url = mStreamResRootPath + "/" + md5;
            }
            else
            {
                url = mStreamResRootPath + "/" + path + "/" + md5;
            }
            return url;
        }

    }
    /// <summary>
    /// 获取主ab资源地址
    /// </summary>
    /// <returns></returns>
    private string GetMainABPath() {
        string url = "";
        VersionAssetData vad = mNameMD5Dic[mMainABPath];
        if (File.Exists( mLocalResRootPath + "/" + vad.md5))
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            url = "File://" + mLocalResRootPath + "/" + vad.md5;
#elif UNITY_ANDROID
            url = "File://" + mLocalResRootPath + "/" + vad.md5;
#elif UNITY_IPHONE
            url = "File://" + mLocalResRootPath + "/" + vad.md5;
#endif
            return url;
        }else{
            url = mStreamResRootPath + "/" + vad.md5;
            return url;
        }
    }
}
