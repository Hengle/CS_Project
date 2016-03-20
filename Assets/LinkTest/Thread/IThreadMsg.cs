using UnityEngine;
using System.Collections;
using System.Threading;
/// <summary>
/// 线程消息基类
/// author wlk
/// </summary>
public abstract class IThreadMsg
{
    // Run In Task Thread
    public virtual void OnBackGroundExcute() { }

    // Run In Task Thread
    public virtual void OnBackGroundExcuteEnd() { }

    // Run In Main Thread
    public virtual void OnEndExcute() { }

    public void SetFailded()
    {
        m_bSuccess = false;
    }

    public void ResetFaildFlag()
    {
        m_bSuccess = true;
    }

    public bool IsSuccess()
    {
        return m_bSuccess;
    }

    public IThreadMsg()
    {
        m_bSuccess = true;
    }

    private bool m_bSuccess;
}

/// <summary>
/// 网络请求基类
/// </summary>
public abstract class IWebGetMsgBase : IThreadMsg
{
    // Run In Task Thread
    public virtual byte[] GetBuffer() { return null; }

    // Run In Task Thread
    public virtual int GetSize() { return 0; }

    public void SetUrl(string url)
    {
        m_url = url;
    }

    public string GetUrl()
    {
        return m_url;
    }

    protected string m_url;
}
