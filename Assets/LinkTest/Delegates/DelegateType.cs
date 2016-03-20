using UnityEngine;
using System.Collections;
/// <summary>
/// 委托类型定义集合（为了方便l#的委托注册，所有的委托类型 都在这里统一声明）
/// author lgr
/// </summary>
public delegate void VoidDelegate();//通用的void参数的委托类型定义
public delegate void StringObjDelegate(string strName, object newData);
public delegate void StringDelegate(string strName);
public delegate void ObjectDelegate(object obj);
public delegate void GameObjectDelegate(GameObject obj);
public delegate void IntDelegate(int num);
public delegate float EasingFunction(float start, float end, float value);

public delegate IEnumerator IEnumeratorDelegate();
