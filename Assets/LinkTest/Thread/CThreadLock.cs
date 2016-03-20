
using System.Threading;

public class CThreadLock
{
	public	CThreadLock ()
	{
		m_lock = new Mutex( false );
	}

	public	bool	Lock()
	{
		return	m_lock.WaitOne();
	}
				
	public	bool	Lock( int ms )
	{
		return	m_lock.WaitOne( ms );
	}

	public	void	UnLock()
	{
		m_lock.ReleaseMutex();
	}

	private	System.Threading.Mutex	m_lock;
}

