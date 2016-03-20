using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextManager : MonoBehaviour {
    //所有Text 文本类型的数字都需要满足一定条件动态显示,例如在游戏中获得分数,+50分,+100分  例如 在游戏中获得金币+1,+2,+3  
    //都需要一个TextMesh 类 去满足 分数的具体显示，和其他类的调用，例如在逻辑上你碰到星星，或者分数，你在star的类中触发getstar，并且调用startext +1
    //这是不合理的,应当遵循MVC模式，把逻辑和显示分割开来
    //关于所有显示都应该在UiManager中进行显示,即TextMesh 应该存在两个接口,一个是供给逻辑代码的调用接口，一个是供给UI界面的显示接口。
    //
    #region Variables

    public string TextName = "";
    public GameObject MainPanel;
    private tk2dTextMesh _textMesh;
    public tk2dTextMesh TextMesh 
    {
        get { return _textMesh; }
    }

    #endregion

    public static Dictionary<string, TextManager> Texts = new Dictionary<string, TextManager>();

    #region Function
    /// <summary>
    /// 此函数进行游戏中金币或者分数的累加
    /// num为金币或者获得分数点的个数，point是每个点的分数
    /// 会进行累加 而且不会清空,需要清空的话调用clear函数
    /// </summary>
    public void TempRecord(int num,int points)
    {
        int point = GetNumber();
        string text = GetString();

        point += num * points;
        _textMesh.text = text + point;
            
    }

    /// <summary>
    /// 此函数进行游戏中金币或者分数的记录
    /// label为playerPrefs的key
    /// </summary>
    public void Record(string label)
    {
        PlayerPrefs.SetInt(label, GetNumber());
    }

    /// <summary>
    /// 此函数进行游戏中金币或者分数的最高值进行的记录
    /// 如果数值没有破纪录自动过滤
    /// label为playerPrefs的key
    /// </summary>
    public void BestRecrd(string label)
    {
         if (PlayerPrefs.GetInt(label, 0) < GetNumber())
        {
            PlayerPrefs.SetInt(label, GetNumber());
        }
    }

    /// <summary>
    /// 显示key为label的value的值
    /// 若出数字以外有别的string不受影响
    /// 只改变数值
    /// </summary>
    public void TextShow(string label)
    {
        _textMesh.text = GetString() + PlayerPrefs.GetInt(label, 0);
    }

    /// <summary>
    /// 此函数进行text的数值清空
    /// 
    /// </summary>
    public void Clear()
    {
        _textMesh.text = GetString()+"0";
    }

    public void TextShowDelay(string text, float interval)
    {
        _textMesh.text = "";
        StartCoroutine(ShowEnumerator(text, interval));
    }

    private IEnumerator ShowEnumerator(string text, float interval)
    {
        foreach (char item in text)
        {
            _textMesh.text += item;
            yield return new WaitForSeconds(interval);
        }
    }

    #endregion

    private int GetNumber()
    {
        string str = _textMesh.text;
        int number = 0;
        string label = null;
        foreach (char item in str)
        {
            if (item >= 48 && item < 58)
            {
                label += item;
            }
        }
        if (label != null) number = int.Parse(label);
        return number;
    }

    private string GetString()
    {
        string str = _textMesh.text;
        string label = null;
        foreach (char item in str)
        {
            if (item < 48 || item >= 58)
            {
                label += item;
            }
        }
        if (label == null) return null;
        return label;
    }

    void OnEnable()
    {
 //       if (!Texts.ContainsKey(TextName)) return;

        Texts.Add(TextName.ToLower(), this);

        if (_textMesh == null)
        {
            _textMesh = MainPanel.GetComponent<tk2dTextMesh>() ?? MainPanel.GetComponentInChildren<tk2dTextMesh>();
        }
        //      this.OnStart += initOnStart;
    }

    /// <summary>
    /// 获取当前的TextManager 实例
    /// </summary>
    public static TextManager GetTextManager(string requestedName)
    {

        requestedName = requestedName.ToLower();
        if (Texts.ContainsKey(requestedName))
        {
            return Texts[requestedName];
        }
        Debug.Log("No texts with that name exists! Are you sure you wrote it correctly?");
        return null;
    }
    

    /// <summary>
    /// 是否显示text
    /// </summary>
    public void SetTextActive(bool value)
    {
        MainPanel.SetActive(value);
    }

    [ContextMenu("test")]
    public void Excute()
    {
    }
}
