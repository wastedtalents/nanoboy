using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(DirectLight2D))]
public class DirectLight2DEditor : Light2DEditor
{
    SerializedProperty beamSize;
    SerializedProperty beamRange;
    SerializedProperty pivotPoint;
    SerializedProperty pivotPointType;
    SerializedProperty uvTiling;
    SerializedProperty uvOffset;

    protected override void SerializeProperties()
    {
        base.SerializeProperties();

        beamSize = serializedObject.FindProperty("beamSize");
        beamRange = serializedObject.FindProperty("beamRange");
        pivotPoint = serializedObject.FindProperty("pivotPoint");
        pivotPointType = serializedObject.FindProperty("pivotPointType");
        uvTiling = serializedObject.FindProperty("uvTiling");
        uvOffset = serializedObject.FindProperty("uvOffset");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(uvTiling);
        EditorGUILayout.PropertyField(uvOffset);

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(beamSize, new GUIContent("Beam Size", ""));
        EditorGUILayout.PropertyField(beamRange, new GUIContent("Beam Range", ""));

        EditorGUILayout.PropertyField(pivotPointType, new GUIContent("Pivot Type", ""));
        GUI.enabled = (pivotPointType.intValue == 2);
        EditorGUILayout.PropertyField(pivotPoint, new GUIContent("Beam Anchor Point", ""));
        GUI.enabled = true;

        if (GUI.changed)
            UpdateLight();
    }

    void OnSceneGUI()
    {
        if (GUI.changed)
            UpdateLight();
    }

    protected override void UpdateLight()
    {
        base.UpdateLight();

        ((DirectLight2D)l).LightBeamSize = beamSize.floatValue;
        ((DirectLight2D)l).LightBeamRange = beamRange.floatValue;
        ((DirectLight2D)l).DiectionalLightPivotPoint = pivotPoint.vector3Value;
        ((DirectLight2D)l).DirectionalPivotPointType = (Light2D.PivotPointType)pivotPointType.intValue;
        ((DirectLight2D)l).UVTiling = uvTiling.vector2Value;
        ((DirectLight2D)l).UVOffset = uvOffset.vector2Value;
    }
}
