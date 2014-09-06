using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Shadow2D))]
public class Shadow2DEditor : Light2DEditor
{
    SerializedProperty sweepStart;
    SerializedProperty sweepSize;
    SerializedProperty lightRadius;

    protected override void SerializeProperties()
    {
        base.SerializeProperties();

        sweepSize = serializedObject.FindProperty("coneAngle");
        sweepStart = serializedObject.FindProperty("coneStart");
        lightRadius = serializedObject.FindProperty("lightRadius");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(sweepStart, new GUIContent("Light Cone Start"));
        EditorGUILayout.PropertyField(sweepSize, new GUIContent("Light Cone Angle", ""));
        sweepSize.floatValue = Mathf.Clamp(sweepSize.floatValue, 0, 360);
        EditorGUILayout.PropertyField(lightRadius);
        lightRadius.floatValue = Mathf.Clamp(lightRadius.floatValue, 0.001f, Mathf.Infinity);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            UpdateLight();
    }

    void OnSceneGUI()
    {
        Handles.color = Color.green;
        float widgetSize = Vector3.Distance(l.transform.position, SceneView.lastActiveSceneView.camera.transform.position) * 0.1f;
        float rad = (((Shadow2D)l).LightRadius);
        Handles.DrawWireDisc(l.transform.position, l.transform.forward, rad);
        lightRadius.floatValue = Mathf.Clamp(Handles.ScaleValueHandle(((Shadow2D)l).LightRadius, l.transform.TransformPoint(Vector3.right * rad), Quaternion.identity, widgetSize, Handles.CubeCap, 1), 0.001f, Mathf.Infinity);

        Handles.color = Color.red;
        Vector3 sPos = l.transform.TransformDirection(Mathf.Cos(Mathf.Deg2Rad * -((((Shadow2D)l).LightConeAngle / 2f) - ((Shadow2D)l).LightConeStart)), Mathf.Sin(Mathf.Deg2Rad * -((((Shadow2D)l).LightConeAngle / 2f) - ((Shadow2D)l).LightConeStart)), 0);
        Handles.DrawWireArc(l.transform.position, l.transform.forward, sPos, ((Shadow2D)l).LightConeAngle, (rad * 0.8f));
        sweepSize.floatValue = Mathf.Clamp(Handles.ScaleValueHandle(((Shadow2D)l).LightConeAngle, l.transform.position - l.transform.right * (rad * 0.8f), Quaternion.identity, widgetSize, Handles.CubeCap, 1), 0, 360);

        Handles.color = new Color(l.LightColor.r, l.LightColor.g, l.LightColor.b, 0.1f);
        Handles.DrawSolidDisc(l.transform.position, Vector3.forward, ((Shadow2D)l).LightRadius);

        if (GUI.changed)
            UpdateLight();
    }

    protected override void UpdateLight()
    {
        base.UpdateLight();

        ((Shadow2D)l).LightConeAngle = sweepSize.floatValue;
        ((Shadow2D)l).LightConeStart = sweepStart.floatValue;
        ((Shadow2D)l).LightRadius = lightRadius.floatValue;
    }
}
