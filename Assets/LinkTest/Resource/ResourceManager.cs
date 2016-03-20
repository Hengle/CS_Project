using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;
/// <summary>
/// resource资源管理器
/// author lgr
/// </summary>

public class ResourceManager : IResManager
{
    
    private StringDelegate OnAssetsLoadComplete = null;
    private VoidDelegate OnAssetsPrepareComplete = null;
    private Dictionary<string, UnityEngine.Object> mResLoaded = null;//加载完成的对象缓存
    
    class LoadTask
    {
        public string callBackEvent;
        public List<string> list;
    }
    private List<LoadTask> mDownLoadQueue = null;//加载任务队列
    void Awake()
    {
        if (IResManager.ResDebug)
        {
            IResManager.Instance = this;
        }

        //DontDestroyOnLoad(gameObject);
        mDownLoadQueue = new List<LoadTask>();//加载队列
        mResLoaded = new Dictionary<string, UnityEngine.Object>();
        //开始加载任务队列
        StartCoroutine(Run());
    }
    /// <summary>
    /// 准备 用于资源更新结束之后调用,回调通知L#脚本
    /// </summary>
    public override void Prepare(VoidDelegate prepareCompleteCallBack, StringDelegate loadCompleteCallBack)
    {
        OnAssetsLoadComplete = loadCompleteCallBack;
        OnAssetsPrepareComplete = prepareCompleteCallBack;
        if (OnAssetsPrepareComplete != null) {
            OnAssetsPrepareComplete();
            OnAssetsPrepareComplete = null;
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
        //Debug.LogError("AddResTask...");
        if (!mDownLoadQueue.Contains(loadTask))
        {
            //Debug.LogError(222);
            mDownLoadQueue.Add(loadTask);
        }
    }
    
    IEnumerator Run()
    {
        while (true)
        {
            if (mDownLoadQueue.Count > 0)
            {
                //Debug.LogError("qian:"+mDownLoadQueue.Count);
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
    IEnumerator LoadAssets(List<string> list)
    {
        mTotalNum = 0;
        mLoadedNum = 0;
        for (int i = 0; i < mDownLoadQueue.Count; i++)
        {
            mTotalNum += mDownLoadQueue[i].list.Count;
        }
        mTotalNum += list.Count;

        for (int j = 0; j < list.Count; j++)
        {
            yield return StartCoroutine(ResLoadAssets(list[j]));
        }
        yield return null;
    }
    IEnumerator ResLoadAssets(string path) {
        yield return null;
        UnityEngine.Object obj = Resources.Load(path);
        if (obj != null)
        {
            if (!mResLoaded.ContainsKey(path))
            {
                mResLoaded.Add(path, obj);
            }
            else {
                LogHelp.LogConsole("key contains in dic:" + path);
            }
            
        }
        mLoadedNum += 1;
    }

    /// <summary>
    /// 加载一个ab资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    private UnityEngine.Object GetRes(string path)
    {
        UnityEngine.Object obj = null;
        if (mResLoaded.ContainsKey(path))
        {
            obj = mResLoaded[path];
        }
        else {
  //          Debug.LogError("has not :" + path);
        }
        return obj;
    }

    public override UnityEngine.Object GetRes(string pathName, string assetName) {
        return GetRes(pathName);
    }

    public override bool HasRes(string path)
    {
        if (mResLoaded.ContainsKey(path)) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 卸载所有已加载的资源包
    /// </summary>
    public override void UnLoadAllRes()
    {
        mResLoaded.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    /// <summary>
    /// 卸载指定的资源包
    /// </summary>
    /// <param name="list"></param>
    public override void UnLoadRes(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (mResLoaded.ContainsKey(list[i])) {
                mResLoaded.Remove(list[i]);
            }
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

}
