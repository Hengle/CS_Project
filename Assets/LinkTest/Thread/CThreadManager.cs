using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum enumThreadID : int
{
    enumThread_Web = 0,
    enumThread_Res = 1,

    enumThread_Receive = 10,
    enumThread_Send = 11,
}

public class CThreadManager
{
    public static CThreadManager GetInstance()
    {
        if (null != CThreadManager.s_Instance)
        {
            return CThreadManager.s_Instance;
        }

        CThreadManager.s_Instance = new CThreadManager();
        return CThreadManager.s_Instance;
    }

    public void OnGetMsg()
    {
        Dictionary<enumThreadID, CThread>.Enumerator itor = m_list.GetEnumerator();
        while (itor.MoveNext())
        {
            itor.Current.Value.GetEndMsg();
        }
    }

    public void PushMsg(enumThreadID id, IThreadMsg msg)
    {

#if 	UNITY_IPHONE
			
			int idIntVal = (int)id;
			if( (int)enumThreadID.enumThread_Res >= idIntVal )
			{
				CMonoThreadSimulator resultIOS = null;
				
				if( ! m_listIOS.TryGetValue( id, out resultIOS ) )
				{
					GameObject webSimulator = new GameObject( "WebMonoSimulator" + ((int)id).ToString() );
					resultIOS = webSimulator.AddComponent< CMonoThreadSimulator >();
					GameObject.DontDestroyOnLoad(webSimulator);
					
					m_listIOS.Add( id, resultIOS );
				}

				resultIOS.push( msg as CThreadMsgWebGet );
				return;
			}
			
#endif

        CThread result = null;
        if (m_list.TryGetValue(id, out result))
        {
            result.PushMsg(msg);
        }
        else
        {
            result = new CThread();
            m_list.Add(id, result);

            result.Start();
            result.PushMsg(msg);
        }


    }

    public void StopAll()
    {
        Dictionary<enumThreadID, CThread>.Enumerator itor = m_list.GetEnumerator();
        while (itor.MoveNext())
        {
            itor.Current.Value.Stop();
        }
        m_list.Clear();
    }

    public void Stop(enumThreadID id)
    {
        CThread result = null;
        if (m_list.TryGetValue(id, out result))
        {
            result.Stop();
            m_list.Remove(id);
        }
    }

    private CThreadManager()
    {

#if 	UNITY_IPHONE		
			
			m_listIOS = new Dictionary< enumThreadID, CMonoThreadSimulator >();	
			
#endif

        m_list = new Dictionary<enumThreadID, CThread>();

    }




#if 	UNITY_IPHONE
	
		private	Dictionary< enumThreadID, CMonoThreadSimulator >	m_listIOS;
			
#endif

    private Dictionary<enumThreadID, CThread> m_list;

    private static CThreadManager s_Instance;
}
