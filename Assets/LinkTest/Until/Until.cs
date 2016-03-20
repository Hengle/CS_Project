using UnityEngine;
using System.Collections;

public class Until{
    private static Vector3 _scale= new Vector3(1,1,1);
    private static Vector3 _angle = Vector3.zero;
    private static Vector3 _temp;

    public static GameObject GameObjInit(GameObject obj,Transform parent)
    {
        obj.transform.parent = parent;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = _scale;
        return obj;
    }

    public static Transform TransformInit(Transform obj, Transform parent)
    {
        obj.parent = parent;
        obj.localPosition = Vector3.zero;
        obj.localRotation = Quaternion.identity;
        obj.localScale = _scale;
        return obj;
    }

    public static void TranslatePosx(Transform obj, float position)
    {
        _temp = obj.localPosition;
        _temp.x = position;
        obj.localPosition = _temp;
    }

    public static void TranslatePosy(Transform obj, float position)
    {
        _temp = obj.localPosition;
        _temp.y = position;
        obj.localPosition = _temp;
    }

    public static void ChangeLocalEulerAngles(Transform transform,float x, float y ,float z)
    {
        _angle.x = x;
        _angle.y = y;
        _angle.z = z;
        transform.localEulerAngles = _angle;
    }


}
