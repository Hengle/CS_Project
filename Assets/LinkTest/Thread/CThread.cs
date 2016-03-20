using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
/// <summary>
/// 线程，主要做一些异步多线程资源加载的事情
/// author wlk
/// </summary>
public class CThread
{
    public CThread()
    {
        m_listTaskArray = new LinkedList<IThreadMsg>();
        m_ListEndTask = new LinkedList<IThreadMsg>();

        m_lockTask = new CThreadLock();
        m_lockEndMsg = new CThreadLock();
    }

    public void Start()
    {
        if (null == m_tIns)
        {
            m_tIns = new System.Threading.Thread(new ThreadStart(this._TMain));
            m_tIns.Start();
        }
    }

    public void Stop()
    {
        m_lockTask.Lock();
        m_listTaskArray.Clear();
        m_lockTask.UnLock();

        m_lockEndMsg.Lock();
        m_ListEndTask.Clear();
        m_lockEndMsg.UnLock();

        if (null != m_tIns)
        {
            m_tIns.Abort();
            m_tIns = null;
        }
    }

    public void GetEndMsg()
    {
        if (m_lockEndMsg.Lock())
        {
            try
            {
                if (0 != m_ListEndTask.Count)
                {
                    IThreadMsg msgResult = m_ListEndTask.First.Value;
                    m_ListEndTask.RemoveFirst();

                    if (null != msgResult)
                    {
                        msgResult.OnEndExcute();
                    }
                    msgResult = null;
                }
            }
            catch
            {
            }
            finally
            {
                m_lockEndMsg.UnLock();
            }
        }
    }

    public void PushMsg(IThreadMsg msg)
    {
        if (null == msg)
        {
            return;
        }

        if (null == m_tIns)
        {
            LogHelp.LogAuto("[Warning] : BackGround Thread Is Not Exist!!");
        }

        try
        {
            m_lockTask.Lock();
            m_listTaskArray.AddLast(msg);
        }
        catch
        {
        }
        finally
        {
            m_lockTask.UnLock();
        }
    }

    private void _TMain()
    {
        for (; ; )
        {
            if (m_lockTask.Lock())
            {
                IThreadMsg msgResult = null;
                try
                {
                    if (0 != m_listTaskArray.Count)
                    {
                        msgResult = m_listTaskArray.First.Value;
                        m_listTaskArray.RemoveFirst();
                    }
                }
                catch
                {
                }
                finally
                {
                    m_lockTask.UnLock();
                }

                if (null != msgResult)
                {
                    try
                    {
                        msgResult.OnBackGroundExcute();
                    }
                    catch
                    {
                        msgResult.SetFailded();
                    }
                    finally
                    {
                        msgResult.OnBackGroundExcuteEnd();

                        m_lockEndMsg.Lock();
                        m_ListEndTask.AddLast(msgResult);
                        m_lockEndMsg.UnLock();

                        System.Threading.Thread.Sleep(30);
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(40);
                }
            }
            else
            {
                System.Threading.Thread.Sleep(30);
            }
        }

    }

    private System.Threading.Thread m_tIns;

    private LinkedList<IThreadMsg> m_listTaskArray;
    private LinkedList<IThreadMsg> m_ListEndTask;

    private CThreadLock m_lockTask;
    private CThreadLock m_lockEndMsg;
}
