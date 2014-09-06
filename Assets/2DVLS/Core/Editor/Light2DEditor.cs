using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

//[InitializeOnLoad]
//public class Initialize2DVLS
//{
//    static Initialize2DVLS()
//    {
//        //File.Exists(EditorApplication.applicationPath)
//    }
//}

[CustomEditor(typeof(Light2D))]
public class Light2DEditor : Editor
{
    protected Light2D l;

    SerializedProperty lightPenetration;
    SerializedProperty lightMaterial;
    SerializedProperty lightDetail;
    SerializedProperty lightColor;
    SerializedProperty useEvents;
    SerializedProperty shadowLayer;

    void OnEnable()
    {
        l = (Light2D)target;
        SerializeProperties();
    }

    protected virtual void SerializeProperties()
    {
        lightDetail = serializedObject.FindProperty("lightDetail");
        lightColor = serializedObject.FindProperty("lightColor");
        lightPenetration = serializedObject.FindProperty("lightPenetration");
        lightMaterial = serializedObject.FindProperty("lightMaterial");
        useEvents = serializedObject.FindProperty("useEvents");
        shadowLayer = serializedObject.FindProperty("shadowLayer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(shadowLayer, new GUIContent("Shadow Layer", "Objects on this layer will cast shadows."));
        EditorGUILayout.PropertyField(lightDetail, new GUIContent("Light Detail", "The number of rays the light checks for when generating shadows. Rays_500 will cast 500 raycasts."));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(lightPenetration);
        EditorGUILayout.PropertyField(lightColor);
        EditorGUILayout.PropertyField(lightMaterial);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(useEvents);
        (target as Light2D).IsStatic = EditorGUILayout.Toggle("Is Static", (target as Light2D).IsStatic);

        EditorGUILayout.Separator();

        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
            UpdateLight();
    }

    int handle = 0;
    void OnSceneGUI()
    {
        Tools.current = Tool.None;
        Event e = Event.current;

        EditorGUI.BeginChangeCheck();
        {
            switch (e.type)
            {
                case EventType.KeyDown:
                    ExecuteKeyDownEvent(e);
                    break;
            }

            if (handle == 0)
            {
                if (Tools.pivotRotation == PivotRotation.Local)
                    l.transform.position = Handles.PositionHandle(l.transform.position, l.transform.rotation);
                else
                    l.transform.position = Handles.PositionHandle(l.transform.position, Quaternion.identity);
            }
            else
            {
                l.transform.rotation = Handles.RotationHandle(l.transform.rotation, l.transform.position);
            }
        }

        SceneView.RepaintAll();
    }

    void ExecuteKeyDownEvent(Event e)
    {
        switch (e.keyCode)
        {
            case KeyCode.W:
                handle = 0;
                break;

            case KeyCode.E:
                break;

            case KeyCode.R:
                handle = 1;
                break;
        }
    }

    protected virtual void UpdateLight()
    {
        Undo.RecordObject(l, "Changed Setting");

        l.LightPenetration = lightPenetration.floatValue;
        l.LightMaterial = (Material)lightMaterial.objectReferenceValue;
        l.LightDetail = (Light2D.LightDetailSetting)lightDetail.intValue;
        l.LightColor = lightColor.colorValue;
        l.EnableEvents = useEvents.boolValue;
        l.ShadowLayer = shadowLayer.intValue;
    }

    //void UndoCall()
    //{
    //    Light2D l2d = (Light2D)target;
    //    //l2d.FlagMeshupdate();
    //}
}
