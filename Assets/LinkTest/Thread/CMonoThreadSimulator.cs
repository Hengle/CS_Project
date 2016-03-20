using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if 	UNITY_IPHONE

public class CMonoThreadSimulator : MonoBehaviour
{
	void	Start()
	{
		m_bInTask = false;
	}
	
	public	void	push( CThreadMsgWebGet msg )
	{
		Check();
		
		m_list.AddLast( msg );
	}
		
	
	void			Update ()
	{
		if( m_bInTask )
		{
			return;
		}
		
		if( null != m_list && 0 < m_list.Count )
		{
			m_bInTask = true;
			CThreadMsgWebGet msg = m_list.First.Value;
			m_list.RemoveFirst();
			
			StartCoroutine( "OnExcute", msg );
		}
	}
	
	IEnumerator 	OnExcute( CThreadMsgWebGet msg )
	{		
		if( string.IsNullOrEmpty( msg.GetUrl() ) )
		{
			msg.SetFailded();
		}
		else
		{
			string urlFormat = msg.GetUrl().Replace( " ", "%20" );

			msg.m_result = new UnityEngine.WWW( urlFormat );
			
			yield return msg.m_result;
			
			if( ! string.IsNullOrEmpty( msg.m_result.error ) )
			{
				msg.SetFailded();
			}
		}

		msg.OnBackGroundExcuteEnd();
		msg.OnEndExcute();
		
		m_bInTask = false;
	}
	
	void	Check()
	{
		if( null == m_list )
		{
			m_list = new LinkedList< CThreadMsgWebGet >();
		}
	}
	
	private	LinkedList< CThreadMsgWebGet >	m_list;
	private	bool						m_bInTask;
}


#endif
