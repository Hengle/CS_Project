using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 资源处理器接口
/// author lgr
/// </summary>

public class IResManager : MonoBehaviour {
    public static IResManager Instance = null;
    public static bool ResDebug = true;//开发调试模式，直接从resource底下加载资源
    protected int mTotalNum = 0;//需要加载的个数
    protected int mLoadedNum = 0;//已经加载的个数
    public int TotalNum
    {
        get
        {
            return mTotalNum;
        }
    }
    public int LoadedNum
    {
        get
        {
            return mLoadedNum;
        }
    }
    public float GetProgress() {
        if (mTotalNum == 0) return 0f;
        return (mLoadedNum + 0f) / (mTotalNum + 0f);
    }
    public virtual void Prepare(VoidDelegate loadCompleteCallBack, StringDelegate prepareCompleteCallBack)
    {
        
    }
    public virtual void AddResTask(List<string> assets,string callBackEvent)
    { 
        
    }
    public virtual UnityEngine.Object GetRes(string abName, string assetName)
    {
        return null;
    }

    public virtual bool HasRes(string path) {
        return true;
    }

    public virtual void UnLoadAllRes()
    { 
        
    }
    public virtual void UnLoadRes(List<string> list)
    { 
        
    }
    public virtual void UnLoadRes(string str)
    {

    }
	// Use this for initialization
	void Awake () {
#if LSHARP_ENV
        ResDebug = false;
#endif
    }
	
}
