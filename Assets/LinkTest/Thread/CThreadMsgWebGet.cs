using UnityEngine;
using System.Collections;
using System.IO;
using System.Net;

public class CThreadMsgWebGet : IWebGetMsgBase
{
    // Run In Task Thread
    public override byte[] GetBuffer()
    {
#if 	UNITY_IPHONE
			
			return	( null != m_result ) ? m_result.bytes : null;
			
#else
        return (null != m_resquestData && null != m_resquestData.memStream) ? m_resquestData.memStream.GetBuffer() : null;

#endif

    }

    // Run In Task Thread
    public override int GetSize()
    {

#if 	UNITY_IPHONE
			
			return	( null != m_result ) ? m_result.size : 0;
			
#else

        return (null != m_resquestData) ? m_resquestData.readLength : 0;

#endif
    }


#if 	! UNITY_IPHONE

    public override void OnBackGroundExcute()
    {
        
        if (string.IsNullOrEmpty(m_url))
        {
            base.SetFailded();
            return;
        }

        m_resquestData = new RequestState();
        string urlFormat = m_url.Replace(" ", "%20");
        WebRequest requestPtr = null;
        if (urlFormat.StartsWith("ftp"))
        {
            FtpWebRequest ftprequestPtr = FtpWebRequest.Create(urlFormat) as FtpWebRequest;
            ftprequestPtr.Timeout = 4000;
            ftprequestPtr.ReadWriteTimeout = 80000;
            ftprequestPtr.KeepAlive = false;
            requestPtr = ftprequestPtr;
        }
        else
        {
            HttpWebRequest httprequestPtr = HttpWebRequest.Create(urlFormat) as HttpWebRequest;
            httprequestPtr.Timeout = 4000;
            httprequestPtr.ReadWriteTimeout = 80000;
            httprequestPtr.KeepAlive = false;
            requestPtr = httprequestPtr;
        }
        Debug.LogError(urlFormat);
        m_resquestData.request = requestPtr;

        try
        {
            m_resquestData.response = m_resquestData.request.GetResponse();
            m_resquestData.streamResponse = m_resquestData.response.GetResponseStream();

            int lenResult = m_resquestData.streamResponse.Read(m_resquestData.BufferRead, 0, BUFFER_SIZE);
            while (0 < lenResult)
            {
                m_resquestData.readLength += lenResult;
                m_resquestData.memStream.Write(m_resquestData.BufferRead, 0, lenResult);
                lenResult = m_resquestData.streamResponse.Read(m_resquestData.BufferRead, 0, BUFFER_SIZE);
            }

            if (0 < m_resquestData.readLength)
            {
                m_resquestData.memStream.Seek(0, SeekOrigin.Begin);
            }
        }
        catch (WebException wex)
        {
            SetFailded();

            //LogHelp.LogAuto(wex.Message.ToString() + wex.Status.ToString());
        }
        catch
        {
            
            SetFailded();
        }
        finally
        {
            if (null != m_resquestData.streamResponse)
            {
                m_resquestData.streamResponse.Close();
                m_resquestData.streamResponse = null;
            }
            if (null != m_resquestData.response)
            {
                m_resquestData.response.Close();
                m_resquestData.response = null;
            }
            if (null != m_resquestData.request)
            {
                m_resquestData.request.Abort();
                m_resquestData.request = null;
            }
        }

    }

#endif


    public override void OnEndExcute()
    {

#if 	UNITY_IPHONE
			
			m_result = null;
			
#else

        if (null != m_resquestData)
        {
            if (null != m_resquestData.memStream)
            {
                m_resquestData.memStream.Close();
                m_resquestData.memStream = null;
            }

            m_resquestData = null;
        }

#endif


    }

    protected CThreadMsgWebGet()
    {

#if 	UNITY_IPHONE
			
			m_result = null;
				
#else

        m_resquestData = null;

#endif

        m_url = string.Empty;
    }


#if 	UNITY_IPHONE
			
		public	UnityEngine.WWW		m_result;
		
		
#else

    private static int BUFFER_SIZE = 1024;

    protected class RequestState
    {
        public byte[] BufferRead;

        public MemoryStream memStream;
        public WebRequest request;
        public WebResponse response;
        public Stream streamResponse;
        public int readLength;

        public RequestState()
        {
            readLength = 0;
            BufferRead = new byte[BUFFER_SIZE];
            request = null;
            streamResponse = null;
            memStream = new MemoryStream();
        }
    }

    private RequestState m_resquestData;
#endif
}