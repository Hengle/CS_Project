using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
/// <summary>
/// 异步非强制资源加载管理器,根据数据库版本判断，如果是需要更新的 那么从服务器下载，写入本地，修改本地数据库，否则直接从本地下载使用
/// author lgr
/// </summary>
public class AysnResManager : MonoBehaviour {
    public static AysnResManager Instance = null;
    private string ResUrl = "";//服务器资源路径
    private string LocalUrl = "";//本地路径

    private Dictionary<string, Object> mDicPool = new Dictionary<string, Object>();//存储非强制的资源单元，目前可能是图片或者音效
    private List<string> mListLoading = new List<string>();//正在加载的资源路径集合[可能是http加载 也可能是www本地加载]

    public void AddTexture2D(string path,Texture2D texture2D) {
        if (!mDicPool.ContainsKey(path))
        {
            mDicPool.Add(path, texture2D);
        }
    }
    public void AddAudio(string path, AudioClip audio) {
        if (!mDicPool.ContainsKey(path))
        {
            mDicPool.Add(path, audio);
        }
    }
    public void RemoveFromLoading(string path) {
        if (mListLoading.Contains(path))
        {
            mListLoading.Remove(path);
        } 
    }
    /// <summary>
    /// 切场景清空引用
    /// </summary>
    public void DestroyPool()
    {
        mListLoading.Clear();
        foreach (string path in mDicPool.Keys)
        {
            mDicPool[path] = null;
        }
        mDicPool.Clear();
    }
    /// <summary>
    /// 获取图片资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Texture2D GetTexture2D(string path)
    {
        if (mDicPool.ContainsKey(path))//已经在缓存中存在了
        {
            return mDicPool[path] as Texture2D;
        }
        if (IResManager.ResDebug){//调试模式下直接下载
            Texture2D texture2D = Resources.Load(path, typeof(Texture2D)) as Texture2D;
            mDicPool.Add(path, texture2D);
            return texture2D;
        }
        
        //缓存不存在 那么看看是否在加载队列中
        if (mListLoading.Contains(path))
        {
            return null;
        }
        //加载
        
        DBVersionEnum versionEnum = DBVersionManager.CompareVersion(path);
        
        int index = path.LastIndexOf("/") + 1;
        string name = path.Substring(index, path.Length - index);

        
        if (versionEnum == DBVersionEnum.None)
        { //本地已经是最新的,那么从本地加载
            AysnLoadTask task = new AysnLoadTask();
            task.name = name;
            task.path = path;
            task.absolutePath = LocalUrl + "/" + path;
            mListLoading.Add(path);
            mLoadingTask.Add(task);
        }
        else { //从服务器加载
            Texture2DMsg texture2DMsg = new Texture2DMsg();
            texture2DMsg.name = path.Substring(index, path.Length - index);
            texture2DMsg.versionEnum = versionEnum;
            texture2DMsg.path = path;
            texture2DMsg.absolutePath = ResUrl + "/" + path;
            texture2DMsg.localPath = LocalUrl + "/" + path;
            texture2DMsg.SetUrl(texture2DMsg.absolutePath);
            CThreadManager.GetInstance().PushMsg(enumThreadID.enumThread_Web, texture2DMsg);
            mListLoading.Add(path);
        }
        return null;
    }
    /// <summary>
    /// 获取音效
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public AudioClip GetAudioClip(string path)
    {
        if (mDicPool.ContainsKey(path))//已经在缓存中存在了
        {
            return mDicPool[path] as AudioClip;
        }
        if (IResManager.ResDebug)
        {//调试模式下直接下载
            AudioClip audio = Resources.Load(path, typeof(AudioClip)) as AudioClip;
            mDicPool.Add(path, audio);
            return audio;
        }

        //缓存不存在 那么看看是否在加载队列中
        if (mListLoading.Contains(path))
        {
            return null;
        }
        //加载

        DBVersionEnum versionEnum = DBVersionManager.CompareVersion(path);

        int index = path.LastIndexOf("/") + 1;
        string name = path.Substring(index, path.Length - index);


        if (versionEnum == DBVersionEnum.None)
        { //本地已经是最新的,那么从本地加载
            AysnLoadTask task = new AysnLoadTask();
            task.name = name;
            task.path = path;
            task.absolutePath = LocalUrl + "/" + path;
            mListLoading.Add(path);
            mLoadingTask.Add(task);
        }
        else
        { //从服务器加载
            AudioMsg audioMsg = new AudioMsg();
            audioMsg.name = path.Substring(index, path.Length - index);
            audioMsg.versionEnum = versionEnum;
            audioMsg.path = path;
            audioMsg.absolutePath = ResUrl + "/" + path;
            audioMsg.localPath = LocalUrl + "/" + path;
            audioMsg.SetUrl(audioMsg.absolutePath);
            CThreadManager.GetInstance().PushMsg(enumThreadID.enumThread_Web, audioMsg);
            mListLoading.Add(path);
        }
        return null;
    }
    /// <summary>
    /// 设置根路径
    /// </summary>
    public void SetResUrl(string url)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ResUrl = url + "unforce/StandaloneWindow";
#elif UNITY_IPHONE
        ResUrl = url + "unforce/IOS";
#elif UNITY_ANDROID
        ResUrl = url + "unforce/Android";
#endif
    }
	void Awake () {
        Instance = this;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        LocalUrl = Application.dataPath + "/../unforce";
        
#elif UNITY_IPHONE
        LocalUrl = Application.persistentDataPath;
#elif UNITY_ANDROID
        LocalUrl = Application.persistentDataPath;
#endif
        StartCoroutine(Run());
    }
    void Update()
    {
        CThreadManager.GetInstance().OnGetMsg();
    }


    IEnumerator Run()
    {
        while (true)
        {
            if (mLoadingTask.Count > 0)
            {
                AysnLoadTask task = mLoadingTask[0];
                mLoadingTask.RemoveAt(0);

                yield return StartCoroutine(LoadTask(task));

                if (mLoadingTask.Count == 0)
                {
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }

            yield return null;
        }
    }

    IEnumerator LoadTask(AysnLoadTask task)
    {
        yield return null;
        WWW www = new WWW("File://" + task.absolutePath);
        yield return www;
        if (www.error == null)
        {
            AssetBundle ab = www.assetBundle;
            Object obj = ab.LoadAsset(task.name);
            mDicPool.Add(task.path, obj);
            ab.Unload(false);
        }
        else
        {
            Debug.LogError("load error:" + www.error+ "  " + task.absolutePath);
        }
        if (mListLoading.Contains(task.path)) {
            mListLoading.Remove(task.path);
        }
        www.Dispose();
        www = null;
    }
    /// <summary>
    /// 本地www加载元数据
    /// </summary>
    class AysnLoadTask
    {
        public string name;//资源名
        public string path;//相对路径
        public string absolutePath;//绝对路径,加载路径
    }
    private List<AysnLoadTask> mLoadingTask = new List<AysnLoadTask>();
}
/// <summary>
/// 图片文件的服务器异步加载
/// </summary>
public class Texture2DMsg : CThreadMsgWebGet {
    public string name;//资源名称
    public string path;//相对路径
    public string absolutePath;//绝对加载路径
    public string localPath;//需要写入的本地路径
    public Texture2D target;
    public DBVersionEnum versionEnum;//版本类型
    private byte[] result;//字节流

    public override void OnBackGroundExcuteEnd()
    {
        result = GetBuffer();
        if (IsSuccess()) {
            if (versionEnum != DBVersionEnum.None)
            {//不是最新的
                ComTool.WriteFile(localPath, result);//写入本地
            }
        }
    }

    public override void OnEndExcute()
    {
        if (IsSuccess()) {
            //更新数据库信息
            CDBInstance localDB = DBVersionManager.GetLocalDBInstance();
            DataBaseInfo info = DBVersionManager.GetDataInfo(path);
            switch (versionEnum)
            {
                case DBVersionEnum.Insert:
                    DBVersionManager.InsertInfo(localDB, "unforceTable", new string[] {
						"'"+path+"'",
						"'"+info.md5+"'",
						info.romote.ToString(),
						info.size.ToString() });
                    break;
                case DBVersionEnum.Update:
                    DBVersionManager.UpdateInfo(localDB, "unforceTable",
                        new string[] {
						"md5",
						"romote",
						"size" },
                        new string[] {
						"'"+info.md5+"'",
						info.romote.ToString(),
						info.size.ToString() }, "path", "'" + path + "'");
                    break;
            }
            if (null != result && 0 < result.Length)
            {
                AssetBundle ab = AssetBundle.LoadFromMemory(result);
                target = ab.LoadAsset(name, typeof(Texture2D)) as Texture2D;
                Debug.LogError(target);

            }
            AysnResManager.Instance.AddTexture2D(path, target);
            AysnResManager.Instance.RemoveFromLoading(path);
            target = null;
        }
        
        base.OnEndExcute();
    }
}
/// <summary>
/// 图片文件的服务器异步加载
/// </summary>
public class AudioMsg : CThreadMsgWebGet
{
    public string name;//资源名称
    public string path;//相对路径
    public string absolutePath;//绝对加载路径
    public string localPath;//需要写入的本地路径
    public AudioClip target;
    public DBVersionEnum versionEnum;//版本类型
    private byte[] result;//字节流

    public override void OnBackGroundExcuteEnd()
    {
        result = GetBuffer();
        if (IsSuccess())
        {
            if (versionEnum != DBVersionEnum.None)
            {//不是最新的
                ComTool.WriteFile(localPath, result);//写入本地
            }
        }
    }

    public override void OnEndExcute()
    {
        if (IsSuccess())
        {
            //更新数据库信息
            CDBInstance localDB = DBVersionManager.GetLocalDBInstance();
            DataBaseInfo info = DBVersionManager.GetDataInfo(path);
            switch (versionEnum)
            {
                case DBVersionEnum.Insert:
                    DBVersionManager.InsertInfo(localDB, "unforceTable", new string[] {
						"'"+path+"'",
						"'"+info.md5+"'",
						info.romote.ToString(),
						info.size.ToString() });
                    break;
                case DBVersionEnum.Update:
                    DBVersionManager.UpdateInfo(localDB, "unforceTable",
                        new string[] {
						"md5",
						"romote",
						"size" },
                        new string[] {
						"'"+info.md5+"'",
						info.romote.ToString(),
						info.size.ToString() }, "path", "'" + path + "'");
                    break;
            }
            if (null != result && 0 < result.Length)
            {
                AssetBundle ab = AssetBundle.LoadFromMemory(result);
                target = ab.LoadAsset(name, typeof(AudioClip)) as AudioClip;
                Debug.LogError(target);

            }
            AysnResManager.Instance.AddAudio(path, target);
            AysnResManager.Instance.RemoveFromLoading(path);
            target = null;
        }

        base.OnEndExcute();
    }
}