using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

public class LinkTest : MonoBehaviour {

    private string URL_SERVER_CONFIG = "http://data.fitfun.net/cdn/DragonFighter/Config/Config.xml";//外网web服务器配置地址
    private string URL_SERVER_RES = "";//资源服务器地址[备注：先读取资源服务器的配置文件，得到一些类似资源地址等配置信息]
    private string URL_SERVER_XML = "unforce/UnforceStandaloneWindowXML.xml";
    public void Awake() {
        StartCoroutine(GetServerConfig());
        
    }

    #region unity3d 方式连接网络
    IEnumerator GetServerConfig()
    {
        Debug.Log("start link");
        ///开始连接,获取配置表
        WWW www = new WWW(ComTool.FormatTickUrl(URL_SERVER_CONFIG));
        yield return www;
        if (www.error == null)
        {
            string content = www.text;
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(content);
                Debug.LogError("get xml succsess");
                XmlNode node = null;

                node = xml.SelectSingleNode("root/ChargeType");
                Debug.LogError(" get id : " + node.Attributes.GetNamedItem("id").Value + " desc :" + node.Attributes.GetNamedItem("desc").Value);

                node = xml.SelectSingleNode("root/AssetUrl");
                URL_SERVER_RES = node.Attributes.GetNamedItem("id").Value;
                Debug.LogError("URL_SERVER_RES :" + URL_SERVER_RES);
            }
            catch (Exception e)
            {
                Debug.LogError("xml format error:" + e.ToString());//xml格式错误
            }
            www.Dispose();
        }
        else
        {
            Debug.LogError("Config www net error:" + www.error);//网络请求失败
            //   StartCoroutine(StartCheck());
        }

        //请求非强制资源配置
        if (URL_SERVER_RES != "") {
            //获得资源配置表
            string path = ComTool.FormatTickUrl(URL_SERVER_RES + URL_SERVER_XML);
            Debug.LogError(path);
            WWW wwwXML = new WWW(path);
            if (wwwXML.error == null)
            {
                string content = wwwXML.text;
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.LoadXml(content);
                    DBVersionManager.SetServerXML(xml);
                    AysnResManager.Instance.SetResUrl(URL_SERVER_RES);
                }
                catch (Exception e)
                {
                    Debug.LogError("unforce xml format error:" + e.ToString());//xml格式错误
                }

                wwwXML.Dispose();
            
            }
            ///
            yield return wwwXML;
        }
    }
    #endregion
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
