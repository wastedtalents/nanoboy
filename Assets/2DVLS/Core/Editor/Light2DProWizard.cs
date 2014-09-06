using UnityEngine;
using UnityEditor;
using System.Collections;

public class Light2DProWizard : ScriptableWizard
{
    [MenuItem(Light2DMenu.baseDirectory + "Setup Pro Shaders", false, 21)]
    static void CreateProWizard()
    {
        camerasInScene = GameObject.FindObjectsOfType<Camera>();
        applyToThisCamera = new bool[camerasInScene.Length];

        for (int i = 0; i < applyToThisCamera.Length; i++)
            applyToThisCamera[i] = true;
        
        ScriptableWizard.DisplayWizard<Light2DProWizard>("Setup Pro Shaders", "Setup");
    }

    [MenuItem(Light2DMenu.baseDirectory + "Setup Pro Shaders", true)]
    static bool ValidateCreateProWizard()
    {
        return UnityEditorInternal.InternalEditorUtility.HasPro();
    }

    void OnWizardCreate()
    {
        if (autoApplyLayerToLights)
        {
            Light2D[] lights = GameObject.FindObjectsOfType<Light2D>();
            foreach (Light2D l in lights)
            {
                if (l.gameObject.layer != 0 && overwriteSetLayers)
                    l.gameObject.layer = lightLayer.value;
                else if (l.gameObject.layer <= 0)
                    l.gameObject.layer = lightLayer.value;
            }
        }

        for (int i = 0; i < camerasInScene.Length; i++)
        {
            if (!applyToThisCamera[i] || camerasInScene[i].gameObject.GetComponent<ProShader>() != null)
                continue;

            camerasInScene[i].cullingMask = 0;// &= ~(1 << lightLayer);

            ProShader r2D = camerasInScene[i].gameObject.AddComponent<ProShader>();
            r2D.lightLayer.value = 1 << lightLayer;
            r2D.renderPassList[0].clearColor = Color.clear;
            r2D.renderPassList[0].layerMask = 1 << backgroundLayer;
        }

        Close();
    }

    static bool[] applyToThisCamera = new bool[0];
    static Camera[] camerasInScene = new Camera[0];

    bool overwriteSetLayers = true;
    bool autoApplyLayerToLights = true;
    LayerMask lightLayer;
    LayerMask backgroundLayer;

    void OnGUI()
    {
        EditorGUILayout.BeginScrollView(Vector2.zero);
        {
            for (int i = 0; i < camerasInScene.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    applyToThisCamera[i] = EditorGUILayout.Toggle(applyToThisCamera[i], GUILayout.MaxWidth(50));
                    EditorGUILayout.ObjectField(camerasInScene[i], typeof(Camera), true);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();

        if (autoApplyLayerToLights = EditorGUILayout.Toggle("Auto Set Layers", autoApplyLayerToLights))
            overwriteSetLayers = EditorGUILayout.Toggle("Overwrite Set Layers: ", overwriteSetLayers);

        lightLayer = EditorGUILayout.LayerField("Light Layer: ", lightLayer);
        backgroundLayer = EditorGUILayout.LayerField("Background Layer: ", backgroundLayer);

        if (GUILayout.Button("Setup"))
            OnWizardCreate();
    }
}
