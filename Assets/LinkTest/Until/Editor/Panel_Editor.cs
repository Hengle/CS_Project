using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

[CustomEditor(typeof(PanelMannger))]

[ExecuteInEditMode]
//请继承Editor

public class Panel_Editor : Editor
{
    PanelMannger _target;
    GUIStyle style = new GUIStyle();
    public static int count = 0;
    private bool showChild = true;

    private float moveTime;
    void OnEnable()
    {
        //i like bold handle labels since I'm getting old:
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        _target = (PanelMannger)target;

        //lock in a default path name:
        if (!_target.initialized)
        {
            _target.initialized = true;
            _target.panelName = "New_Panel " + ++count;
            _target.initialName = _target.panelName;
        }
    }
    
    public override void OnInspectorGUI()
    {
        //path name:
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Panel Name");
        _target.panelName = EditorGUILayout.TextField(_target.panelName);
        EditorGUILayout.EndHorizontal();

        if (_target.panelName == "")
        {
            _target.panelName = _target.initialName;
        }

        //Main panel
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Main Panel");
        _target.mainPanel = (GameObject)EditorGUILayout.ObjectField(_target.mainPanel, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();
        //MoveEnable
        _target.mainMoveAble = EditorGUILayout.BeginToggleGroup("MoveEnabled", _target.mainMoveAble);
        if (_target.mainMoveAble)
        {
            

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("EaseType");
            _target.moveEasetype = (iTween.EaseType)EditorGUILayout.EnumPopup(_target.moveEasetype);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            _target.position_islocal = EditorGUILayout.Toggle("islocal", _target.position_islocal);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.move_ignoretimescale = EditorGUILayout.Toggle("ignoretimescale", _target.move_ignoretimescale);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            _target.m_time = EditorGUILayout.Toggle("time", _target.m_time);
            if (_target.m_time)
            {
                _target.mTime = EditorGUILayout.FloatField(_target.mTime);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.m_speed = EditorGUILayout.Toggle("speed", _target.m_speed);
            if (_target.m_speed)
            {
                _target.mSpeed = EditorGUILayout.FloatField(_target.mSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.m_delay = EditorGUILayout.Toggle("delay", _target.m_delay);
            if (_target.m_delay)
            {
                _target.mDelay = EditorGUILayout.FloatField(_target.mDelay);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.pathEnable = EditorGUILayout.BeginToggleGroup("path", _target.pathEnable);
            if (_target.pathEnable)
            {
                var index = 0;
                string pathName = "";
                var paths = (GameObject.FindObjectsOfType(typeof(iTweenPath)) as iTweenPath[]);
                if (0 == paths.Length)
                {
                    pathName = "";
                    GUILayout.Label("No paths are defined");
                }
                else
                {
                    for (var i = 0; i < paths.Length; ++i)
                    {
                        if (paths[i].pathName == pathName)
                        {
                            index = i;
                        }
                    }
                    index = EditorGUILayout.Popup(index, (GameObject.FindObjectsOfType(typeof(iTweenPath)) as iTweenPath[]).Select(path => path.pathName).ToArray());

                    pathName = paths[index].pathName;
                }
                
            }
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndHorizontal();
                if (!_target.pathEnable) 
            {
                EditorGUILayout.BeginHorizontal();
                _target.FromP = EditorGUILayout.Vector3Field("FromPoint", _target.FromP);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                _target.ToP = EditorGUILayout.Vector3Field("ToPoint", _target.ToP);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndToggleGroup();

        //RotateEnable
        _target.mainRotateAble = EditorGUILayout.BeginToggleGroup("RotateEnabled", _target.mainRotateAble);

        if (_target.mainRotateAble)
        {
            EditorGUILayout.BeginHorizontal();
            _target.FromR = EditorGUILayout.Vector3Field("FromRotate", _target.FromR);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.ToR = EditorGUILayout.Vector3Field("ToRotate", _target.ToR);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("EaseType");
            _target.rotateEasetype = (iTween.EaseType)EditorGUILayout.EnumPopup(_target.rotateEasetype);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            _target.rotation_islocal = EditorGUILayout.Toggle("islocal", _target.rotation_islocal);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.rotate_ignoretimescale = EditorGUILayout.Toggle("ignoretimescale", _target.rotate_ignoretimescale);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            _target.r_time = EditorGUILayout.Toggle("time", _target.r_time);
            if (_target.r_time)
            {
                _target.rTime = EditorGUILayout.FloatField(_target.rTime);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.r_speed = EditorGUILayout.Toggle("speed", _target.r_speed);
            if (_target.r_speed)
            {
                _target.rSpeed = EditorGUILayout.FloatField(_target.rSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.r_delay = EditorGUILayout.Toggle("delay", _target.r_delay);
            if (_target.r_delay)
            {
                _target.rDelay = EditorGUILayout.FloatField(_target.rDelay);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndToggleGroup();


        //ScaleEnable
        _target.mainScaleAbel = EditorGUILayout.BeginToggleGroup("ScaleEnabled", _target.mainScaleAbel);
        if (_target.mainScaleAbel)
        {
            EditorGUILayout.BeginHorizontal();
            _target.FromS = EditorGUILayout.Vector3Field("FromScale", _target.FromS);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.ToS = EditorGUILayout.Vector3Field("ToScale", _target.ToS);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("EaseType");
            _target.scaleEasetype = (iTween.EaseType)EditorGUILayout.EnumPopup(_target.scaleEasetype);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            _target.scale_ignoretimescale = EditorGUILayout.Toggle("ignoretimescale", _target.scale_ignoretimescale);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            _target.s_time = EditorGUILayout.Toggle("time", _target.s_time);
            if (_target.s_time)
            {
                _target.sTime = EditorGUILayout.FloatField(_target.sTime);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.s_speed = EditorGUILayout.Toggle("speed", _target.s_speed);
            if (_target.s_speed)
            {
                _target.sSpeed = EditorGUILayout.FloatField(_target.sSpeed);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _target.s_delay = EditorGUILayout.Toggle("delay", _target.s_delay);
            if (_target.s_delay)
            {
                _target.sDelay = EditorGUILayout.FloatField(_target.sDelay);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndToggleGroup();

        showChild = EditorGUILayout.Foldout(showChild,"Child_Objs");
        if (showChild)
        {
            //exploration segment count control:
		    EditorGUILayout.BeginHorizontal();
		    EditorGUILayout.PrefixLabel("Child Count");
		    _target.childCount =  Mathf.Clamp(EditorGUILayout.IntSlider(_target.childCount, 0, 20), 2,100);
		    EditorGUILayout.EndHorizontal();

            //add node?
            if (_target.childCount > _target.childObjs.Count)
            {
                for (int i = 0; i < _target.childCount - _target.childObjs.Count; i++)
                {
                    _target.childObjs.Add(null);
                }
            }

            //remove node?
            if (_target.childCount < _target.childObjs.Count)
            {
                if (EditorUtility.DisplayDialog("Remove path node?", "Shortening the node list will permantently destory parts of your path. This operation cannot be undone.", "OK", "Cancel"))
                {
                    int removeCount = _target.childObjs.Count - _target.childCount;
                    _target.childObjs.RemoveRange(_target.childObjs.Count - removeCount, removeCount);
                }
                else
                {
                    _target.childCount = _target.childObjs.Count;
                }
            }

            //child_objs display:
            EditorGUI.indentLevel = 1;
            for (int i = 0; i < _target.childCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Child" + (i + 1));
                _target.childObjs[i] = (GameObject)EditorGUILayout.ObjectField(_target.childObjs[i], typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }
        }

        //update and redraw:
        if (GUI.changed)
        {
            EditorUtility.SetDirty(_target);
        } 
    }
}
