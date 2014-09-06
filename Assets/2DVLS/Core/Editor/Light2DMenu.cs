using UnityEngine;
using UnityEditor;
using System.Collections;

public class Light2DMenu : Editor
{
    public const string baseDirectory = "GameObject/Create Other/2DVLS/";
    public static bool HasPro { get { return UnityEditorInternal.InternalEditorUtility.HasPro(); } }

    // *************************** 2D LIGHTS ************************************
    [MenuItem(baseDirectory + "2D/Add Radial Light", false, 41)]
    public static void CreateNewRadialLight()
    {
        Material mat = Resources.Load<Material>(HasPro ? "RadialLight" : "RadialLight_NonPro");
        RadialLight2D light = RadialLight2D.Create(GetPlacement(), new Color(1f, 0.6f, 0f, 1f), GetSize(), 360, 0, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 2D Radial Light");
    }

    [MenuItem(baseDirectory + "2D/Add Spot Light", false, 42)]
    public static void CreateNewSpotLight()
    {
        Material mat = Resources.Load<Material>(HasPro ? "RadialLight" : "RadialLight_NonPro");
        RadialLight2D light = RadialLight2D.Create(GetPlacement(), new Color(1f, 0.6f, 0f, 1f), GetSize(), 45, 0, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 2D Radial Light");
    }

    [MenuItem(baseDirectory + "2D/Add Directional Light", false, 43)]
    public static void CreateNewDirectionalLight()
    {
        Material mat = Resources.Load<Material>(HasPro ? "DirectionalLight" : "DirectionalLight_NonPro");
        DirectLight2D light = DirectLight2D.Create(GetPlacement(), new Color(1f, 0.6f, 0f, 1f), 20, 5, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 2D Radial Light");
    }

    [MenuItem(baseDirectory + "2D/Add Shadow Emitter", false, 44)]
    public static void CreateNewShadowEmitter()
    {
        Material mat = Resources.Load<Material>(HasPro ? "Shadow" : "Shadow");
        Shadow2D light = Shadow2D.Create(GetPlacement(), new Color(0, 0, 0f, 1f), GetSize(), 360, 0, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 2D Shadow Emitter");
    }


    // ********************** 3D LIGHTS ******************************************
    [MenuItem(baseDirectory + "3D/Add Radial Light", false, 45)]
    public static void CreateNewRadial3DLight()
    {
        Material mat = Resources.Load<Material>(HasPro ? "RadialLight" : "RadialLight_NonPro");
        RadialLight3D light = RadialLight3D.Create(GetPlacement(), new Color(1f, 0.6f, 0f, 1f), GetSize(), 360, 0, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 3D Radial Light");
    }

    [MenuItem(baseDirectory + "3D/Add Spot Light", false, 46)]
    public static void CreateNew3DSpotLight()
    {
        Material mat = Resources.Load<Material>(HasPro ? "RadialLight" : "RadialLight_NonPro");
        RadialLight3D light = RadialLight3D.Create(GetPlacement(), new Color(1f, 0.6f, 0f, 1f), GetSize(), 45, 0, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 3D Spot Light");
    }

    [MenuItem(baseDirectory + "3D/Add Shadow Emitter", false, 47)]
    public static void CreateNew3DShadowEmitter()
    {
        Material mat = Resources.Load<Material>(HasPro ? "Shadow" : "Shadow");
        Shadow3D light = Shadow3D.Create(GetPlacement(), new Color(0, 0, 0f, 1f), GetSize(), 360, 0, mat);
        light.ShadowLayer = -1;

        Selection.activeGameObject = light.gameObject;
        Undo.RegisterCreatedObjectUndo(light.gameObject, "Created 3D Shadow Emitter");
    }

    // ********************** HELP FILES ***************************    
    [MenuItem(baseDirectory + "Help/Documentation")]
    public static void SeekHelp_Documentation()
    {
        Application.OpenURL("http://reverieinteractive.com/2DVLS/Documentation3/");
    }

    [MenuItem(baseDirectory + "Help/Online Contact Form")]
    public static void SeekHelp_Form()
    {
        Application.OpenURL("http://reverieinteractive.com/contact/");
    }

    [MenuItem(baseDirectory + "Help/Unity Forum Thread")]
    public static void SeekHelp_UnityForum()
    {
        Application.OpenURL("http://forum.unity3d.com/threads/142532-2D-Mesh-Based-Volumetric-Lights");
    }

    [MenuItem(baseDirectory + "Help/Direct [reveriejake@gmail.com]")]
    public static void SeekHelp_Direct()
    {
        Application.OpenURL("mailto:reveriejake87@gmail.com");
    }

    private static Vector3 GetPlacement()
    {
        Camera c = SceneView.currentDrawingSceneView.camera;
        if(c != null)
        {
            if (SceneView.currentDrawingSceneView.in2DMode)
                return c.transform.position + new Vector3(0, 0, -c.transform.position.z);
            else
                return c.transform.position + c.transform.forward * 15f;
        }
        else
            return Vector3.zero;
    }

    private static float GetSize()
    {
        if (SceneView.currentDrawingSceneView.in2DMode)
            return 5;
        else
            return 1;
    }
}