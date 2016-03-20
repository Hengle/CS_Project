#region Namespaces
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion


public class PanelMannger : MonoBehaviour
{
    //此属性在编辑界面编辑
    #region Variables
    //所有的变量属性
    public delegate void PanelEvent();
    public event PanelEvent moveOnStart, moveOnUpdate, moveOnCompelete;
    public event PanelEvent rotateOnStart, rotateOnUpdate, rotateOnCompelete;
    public event PanelEvent scaleOnStart, scaleOnUpdate, scaleOnCompelete;

    public iTween.EaseType moveEasetype = iTween.EaseType.linear, rotateEasetype = iTween.EaseType.linear, scaleEasetype = iTween.EaseType.linear;
    private Hashtable moveHash = new Hashtable();
    public string panelName = "";
    public GameObject mainPanel,onstartTarget,onupdateTarget,oncompleteTarget;
    public new Vector3 FromP, ToP,FromR,ToR,FromS,ToS;
    public List<GameObject> childObjs = new List<GameObject>();
    public List<Vector3> paths;
    public float mTime, rTime, sTime, mSpeed, rSpeed, sSpeed ,mDelay,rDelay,sDelay;
    public int childCount;

    public bool mainMoveAble, mainRotateAble, mainScaleAbel, position_islocal, rotation_islocal;
    public bool move_ignoretimescale = true, rotate_ignoretimescale =true , scale_ignoretimescale =true;
    public bool initialized = false;
    public bool pathEnable, m_delay, r_delay, s_delay, m_speed, r_speed, s_speed, m_time, r_time, s_time;
    
    public string initialName = "";
    public static Dictionary<string, PanelMannger> panels = new Dictionary<string, PanelMannger>();


    #endregion

    //在初始化的时候 保存至本地.
    void OnEnable()
    {
        iTween.Init(mainPanel);
        panels.Add(panelName.ToLower(), this);
    }

	void OnDestroy()
	{
		panels.Remove(panelName.ToLower());
	}

    //通过PanelManager.GetPanelManager(name) 获取实例
    public static PanelMannger GetPanelManager(string requestedName) {
       
        requestedName = requestedName.ToLower();
        if (panels.ContainsKey(requestedName))
        {
            return panels[requestedName];
        }
        else
        {
            Debug.Log("No panels with that name exists! Are you sure you wrote it correctly?");
            return null;
        }
    }
    #region excute
    //具体对itween的进行的封装.除非有特殊需求进行修改,不然不用管。
    void BeforeStart()
    {
        if(!mainPanel.activeSelf){
            mainPanel.SetActive(true);
        }
        
        if (mainMoveAble)
        {
            moveHash.Clear();

            if (position_islocal)
            {
                mainPanel.transform.localPosition = FromP;
                moveHash.Add("islocal", position_islocal);
            }
            else
            {
                mainPanel.transform.position = FromP;
            }

            if (pathEnable)
            {
                moveHash.Add("path", paths);
                
            }
            else 
            {
                moveHash.Add("position", ToP);
            }

            moveHash.Add("easetype", moveEasetype);

            if (moveOnStart != null)
            {
 //             moveHash.Add("onstarttarget", onstartTarget);
                moveHash.Add("onstart", "TriggerMoveOnStart");
            }

            if(moveOnUpdate !=null){
                moveHash.Add("onupdate", "TriggerMoveOnUpdate");
  //              moveHash.Add("onupdatetarget", onupdateTarget);
            }

            if(moveOnCompelete != null){
                moveHash.Add("oncomplete", "TriggerMoveOnCompelete");
 //               moveHash.Add("oncompletetarget", oncompleteTarget);
            }

            if (move_ignoretimescale)
            {
                moveHash.Add("ignoretimescale", move_ignoretimescale);
            }
            if (m_speed)
            {
                moveHash.Add("speed", mSpeed);
                
            }
            if (m_time)
            {
                moveHash.Add("time", mTime);
            }

            if (m_delay)
            {
                moveHash.Add("delay", mDelay);
            }



            iTween.MoveTo(mainPanel, moveHash);
        }

        if (mainRotateAble) {
            Hashtable theHash = new Hashtable();
            theHash.Add("rotation", ToR);

            if (rotateEasetype.ToString() != "linear")
            {
                theHash.Add("easetype", rotateEasetype);
            }

            if (r_delay)
            {
                theHash.Add("delay", rDelay);
            }

            if (rotation_islocal)
            {
                theHash.Add("islocal", rotation_islocal);
                mainPanel.transform.localEulerAngles = FromR;
            }
            else
            {
                mainPanel.transform.eulerAngles = FromR;
            }

            if (rotateOnStart != null)
            {
                moveHash.Add("onstart", "TriggerRotateOnStart");
            }

            if (rotateOnUpdate != null)
            {
                moveHash.Add("onupdate", "TriggerRotateOnUpdate");
            }

            if (rotateOnCompelete != null)
            {               
                moveHash.Add("oncomplete", "TriggerRotateOnCompelete");
            }

            if (rotate_ignoretimescale)
            {
                theHash.Add("ignoretimescale", rotate_ignoretimescale);
            }
            if (r_speed)
            {
                theHash.Add("speed", rSpeed);
            }
            if(r_time)
            {
                theHash.Add("time", rTime);
            }
            iTween.RotateTo(mainPanel, theHash);
        }


        if(mainScaleAbel){
            Hashtable theHash = new Hashtable();

            mainPanel.transform.localScale = FromS;    

            theHash.Add("scale", ToS);

            if (scaleEasetype.ToString() != "linear")
            {
                theHash.Add("easetype", scaleEasetype);
            }

            if (s_delay)
            {
                theHash.Add("delay", sDelay);
            }

            if (scaleOnStart != null)
            {
                theHash.Add("onstarttarget", gameObject);
                theHash.Add("onstart", "TriggerScaleOnStart");
            }

            if (scaleOnUpdate != null)
            {
                theHash.Add("onupdatetarget", gameObject);
                theHash.Add("onupdate", "TriggerScaleOnUpdate");
            }

            if (scaleOnCompelete != null)
            {
                theHash.Add( "oncompletetarget", gameObject);
                theHash.Add("oncomplete", "TriggerScaleOnCompelete");
            }
    
			Debug.Log(scale_ignoretimescale);
            if (scale_ignoretimescale)
            {
				theHash.Add("ignoretimescale", scale_ignoretimescale);
            }
            if (s_speed)
            {
                theHash.Add("speed", sSpeed);
                
            }
            if(s_time)
            {
                theHash.Add("time", sTime);
            }
            iTween.ScaleTo(mainPanel, theHash);
        }

    } 

    public void Stop() {
        Component[] tweens = mainPanel.GetComponents(typeof(iTween));
        if (tweens.Length != 0)
        {
            foreach (var component in tweens)
            {
                var item = (iTween) component;
                Destroy(item);
            }
        }
    }
    #endregion

    #region events
    //所有委托事件,在itween start update complete 的时候调用.只要函数不为空就会自动添加 在itween执行时执行.
    public void TriggerMoveOnStart()
    {
        if (moveOnStart != null)
        {
            moveOnStart();
            Debug.Log("OnStart");
        }
    }

    public void TriggerMoveOnUpdate()
    {
        if (moveOnUpdate != null)
        {
            moveOnUpdate();
            Debug.Log("OnUpdate");
        }
    }

    public void TriggerMoveOnCompelete()
    {
        if (moveOnCompelete != null)
        {
            moveOnCompelete();
            Debug.Log("OnCompelete");
        }
    }
    public void TriggerRotateOnStart()
    {
        if (rotateOnStart != null)
        {
            rotateOnStart();
            Debug.Log("OnStart");
        }
    }

    public void TriggerRotateOnUpdate()
    {
        if (rotateOnUpdate != null)
        {
            rotateOnUpdate();
            Debug.Log("OnUpdate");
        }
    }

    public void TriggerRotateOnCompelete()
    {
        if (rotateOnCompelete != null)
        {
            rotateOnCompelete();
            Debug.Log("OnCompelete");
        }
    }
    public void TriggerScaleOnStart()
    {
        if (scaleOnStart != null)
        {
            scaleOnStart();
            Debug.Log("OnStart");
        }
    }

    public void TriggerScaleOnUpdate()
    {
        if (scaleOnUpdate != null)
        {
            scaleOnUpdate();
            Debug.Log("OnUpdate");
        }
    }

    public void TriggerScaleOnCompelete()
    {
        if (scaleOnCompelete != null)
        {
            scaleOnCompelete();
           
        }
    }
    #endregion
    //函数的最后执行方法
    [ContextMenu("excute")]
    public void Excute() {
   //     destroy(mainPanel);
		BeforeStart();
    }

	public void Hide() {
		//     destroy(mainPanel);
		mainPanel.SetActive (false);
	}
}
