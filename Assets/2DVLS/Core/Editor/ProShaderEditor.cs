using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(ProShader))]
public class ProShaderEditor : Editor 
{
    SerializedProperty useProjectAmbientColor;
    SerializedProperty ambientColor;
    SerializedProperty lightLayer;
    SerializedProperty renderPassList;

    SerializedProperty blendMat;
    SerializedProperty blurMat;
    SerializedProperty alphaMat;

    bool showGroup = false;

    void OnEnable()
    {
        useProjectAmbientColor = serializedObject.FindProperty("useProjectAmbientColor");
        ambientColor = serializedObject.FindProperty("ambientColor");
        lightLayer = serializedObject.FindProperty("lightLayer");
        renderPassList = serializedObject.FindProperty("renderPassList");
        blendMat = serializedObject.FindProperty("_blendMat");
        blurMat = serializedObject.FindProperty("_blurMat");
        alphaMat = serializedObject.FindProperty("_alphaMat");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(lightLayer, new GUIContent("Light Layer", "The layers that your lights are set to."));
        EditorGUILayout.PropertyField(useProjectAmbientColor, new GUIContent("Use Ambient Light", ""));

        EditorGUILayout.Space();

        if (!useProjectAmbientColor.boolValue)
            EditorGUILayout.PropertyField(ambientColor, new GUIContent("Ambient Color", "Sets the ambient color of the scene."));

        if (showGroup = EditorGUILayout.Foldout(showGroup, new GUIContent("Shader Group", "What materials to use when blending the layers")))
        {
            EditorGUILayout.PropertyField(blendMat);
            EditorGUILayout.PropertyField(blurMat);
            EditorGUILayout.PropertyField(alphaMat);
        }

        EditorGUILayout.Space();

        if (renderPassList.arraySize == 0)
            EditorGUILayout.HelpBox("No render layers currently assigned!", MessageType.Error);

        EditorListVLS.Show(renderPassList);

        serializedObject.ApplyModifiedProperties();
    }
}
