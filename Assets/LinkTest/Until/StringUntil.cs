using System;
using UnityEngine;
using System.Collections;

public class StringUntil
{

    public static int GetNumber(String text)
    {
        string str = text;
        int number = 0;
        string label = null;
        foreach (char item in str)
        {
            if (item >= 48 && item < 58)
            {
                label += item;
//				Debug.Log(item+"     "+label);
            }
        }
        if (label != null) number = int.Parse(label);
        return number;
    }
}
