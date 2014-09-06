//#define HIDE_MESH_WIREFRAME     // Comment out this define to show mesh wireframes
#define HIDE_MESH_BOUNDS        // Comment out this define to show the mesh bounds of the lights
#define USE_LATEUPDATE_LOOP     // Comment out this define to use 'Update' instead of 'LateUpdate'
#define HIDE_MESH_FILTER        // Comment out this define to show mesh filter in the editor
#define HIDE_MESH_RENDERER      // Comment out this define to show mesh renderer in editor

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LightEventListenerType { OnEnter, OnStay, OnExit }
public delegate void Light2DEvent(Light2D lightObject, GameObject objectInLight);

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public abstract class Light2D : MonoBehaviour
{
    public enum PivotPointType
    {
        Center,
        End,
        Custom
    }

    public enum LightDetailSetting
    {
        Rays_50 = 48,
        Rays_100 = 96,
        Rays_200 = 192,
        Rays_300 = 288,
        Rays_400 = 384,
        Rays_500 = 480,
        Rays_600 = 576,
        Rays_700 = 672,
        Rays_800 = 816,
        Rays_900 = 912,
        Rays_1000 = 1008,
        Rays_2000 = 2016,
        Rays_3000 = 3024,
        Rays_4000 = 4032,
        Rays_5000 = 5040
    }

    public enum LightTypeSetting
    {
        Radial,
        Directional
    }

    public enum LightDimensionSupport
    {
        _2D,
        _3D
    }

    protected List<GameObject> identifiedObjects = new List<GameObject>();
    protected List<GameObject> unidentifiedObjects = new List<GameObject>();

    public static event Light2DEvent OnBeamEnter = null;
    public static event Light2DEvent OnBeamStay = null;
    public static event Light2DEvent OnBeamExit = null;

    protected LightTypeSetting lightType;
    protected LightDimensionSupport lightDimensionSupport = LightDimensionSupport._2D;

    protected Mesh _mesh;

    private MeshRenderer _renderer;
    private MeshFilter _filter;

    private float kColCount = 5000;
    private float[] pCheckChange = new float[0];

    protected Vector3[] prevPoints = new Vector3[3];
    protected Quaternion lookAtRotation = Quaternion.identity;
    protected Collider2D[] _2DObjList;
    protected Collider[] _3DObjList;

    protected List<int> tris = new List<int>();
    protected List<Vector3> verts = new List<Vector3>();
    protected List<Vector3> normals = new List<Vector3>();
    protected List<Vector2> uvs = new List<Vector2>();
    protected List<Color32> colors = new List<Color32>();
    
    [SerializeField]
    protected Color lightColor = new Color(0.8f, 1f, 1f, 0);
    [SerializeField]
    protected LightDetailSetting lightDetail = LightDetailSetting.Rays_300;
    [SerializeField]
    protected Material lightMaterial;
    [SerializeField]
    protected LayerMask shadowLayer = -1; // -1 = EVERYTHING, 0 = NOTHING, 1 = DEFAULT
    [SerializeField]
    protected bool useEvents = false;
    [SerializeField]
    protected bool lightEnabled = true;
    [SerializeField]
    protected float lightPenetration = 0f;

    /// <summary>Gets the type of light that has been created.</summary>
    public LightTypeSetting LightType { get { return lightType; } }
    /// <summary>Sets the depth of which the light should penetrate objects.</summary>
    public float LightPenetration { get { return lightPenetration; } set { lightPenetration = value; flagMeshUpdate = true; } }
    /// <summary>Sets the Color of the light.</summary>
    public Color LightColor { get { return lightColor; } set { lightColor = value; InternalUpdateColors(); } }
    /// <summary>Sets the ray count when the light is finding shadows.</summary>
    public LightDetailSetting LightDetail { get { return lightDetail; } set { lightDetail = value; flagNormalsUpdate = true; flagMeshUpdate = true; } }
    /// <summary>Sets the lights material. Best to use the 2DVLS shaders or the Particle shaders.</summary>
    public Material LightMaterial { get { return lightMaterial; } set { lightMaterial = value; InternalUpdateMaterial(); } }
    /// <summary>The layer which responds to the raycasts. If a collider is on the same layer then a shadow will be cast from that collider</summary>
    public LayerMask ShadowLayer { get { return shadowLayer; } set { shadowLayer = value; flagMeshUpdate = true; } }
    /// <summary>When set to 'TRUE' the light will use events such as 'OnBeamEnter(Light2D, GameObject)', 'OnBeamStay(Light2D, GameObject)', and 'OnBeamExit(Light2D, GameObject)'</summary>
    public bool EnableEvents { get { return useEvents; } set { useEvents = value; } }
    /// <summary>Returns 'TRUE' when light is enabled</summary>
    public bool LightEnabled { get { return lightEnabled; } set { if (value != lightEnabled) { lightEnabled = value; /*if (isShadowCaster) UpdateMesh_RadialShadow(); else UpdateMesh_Radial();*/ } } }
    /// <summary>Returns 'TRUE' when light is visible</summary>
    public bool IsVisible { get { return (_renderer) ? _renderer.isVisible : false; } }
    /// <summary>Sets the light to static. Alternativly you can use the "gameObject.isStatic" method or tick the static checkbox in the inspector.</summary>
    public bool IsStatic { get { return gameObject.isStatic; } set { gameObject.isStatic = value; } }

    protected bool flagMeshUpdate = true;
    protected bool flagUVUpdate = true;
    protected bool flagNormalsUpdate = true;
    protected bool flagInitialized = false;

    void OnDrawGizmosSelected()
    {
#if !HIDE_MESH_BOUNDS
        if (_renderer)
        {
            Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireCube(_renderer.bounds.center, _renderer.bounds.size);
        }
#endif
    }

    void Awake()
    {
        Initialize();
    }

    void OnEnable()
    {
        Reset();

        LightEnabled = true;

        _filter = gameObject.GetComponent<MeshFilter>();
        _renderer = gameObject.GetComponent<MeshRenderer>();

#if HIDE_MESH_FILTER
        _filter.hideFlags = HideFlags.HideInInspector;
#endif

#if HIDE_MESH_RENDERER
        _renderer.hideFlags = HideFlags.HideInInspector;
#endif

        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "LightMesh_" + gameObject.GetInstanceID();
            _mesh.hideFlags = HideFlags.HideAndDontSave;
        }

        _renderer.material = lightMaterial;

#if UNITY_EDITOR
#if HIDE_MESH_WIREFRAME
        UnityEditor.EditorUtility.SetSelectedWireframeHidden(_renderer, true);
#else
        UnityEditor.EditorUtility.SetSelectedWireframeHidden(_renderer, false);
#endif
#endif
    }

    void OnDisable()
    {
        LightEnabled = false;

#if UNITY_EDITOR
        DestroyImmediate(_mesh);
#else
        Destroy(_mesh);
#endif
    }

    void Reset()
    {
        flagMeshUpdate = true;
        flagUVUpdate = true;
        flagNormalsUpdate = true;
        flagInitialized = false;
    }

#if USE_LATEUPDATE_LOOP
    void LateUpdate()
    {
        //PreUpdate();
        InternalUpdate();
    }
#else
    void Update()
    {
        //PreUpdate();
        InternalUpdate();
    }
#endif

    protected virtual void Initialize() { }

    protected abstract void Draw();

    protected abstract void CollectColliders();

    protected abstract void UpdateTriangles();

    protected abstract void UpdateUVs();
    
    void InternalUpdate()
    {
        if (_renderer)
        {
            if (Application.isPlaying && (IsStatic && flagInitialized))
                return;

            _renderer.enabled = lightEnabled;
            CollectColliders();

            switch(lightDimensionSupport)
            {
                case LightDimensionSupport._2D:
                    InternalUpdate2DObjects();
                    break;
                case LightDimensionSupport._3D:
                    InternalUpdate3DObjects();
                    break;
            }

            if(flagMeshUpdate)
                InternalUpdateMesh();

            if (Application.isPlaying && useEvents)
            {
                for (int i = 0; i < unidentifiedObjects.Count; i++)
                {
                    if (identifiedObjects.Contains(unidentifiedObjects[i]))
                    {
                        if (OnBeamStay != null)
                            OnBeamStay(this, unidentifiedObjects[i]);
                    }
                    else
                    {
                        identifiedObjects.Add(unidentifiedObjects[i]);

                        if (OnBeamEnter != null)
                            OnBeamEnter(this, unidentifiedObjects[i]);
                    }
                }

                for (int i = 0; i < identifiedObjects.Count; i++)
                {
                    if (!unidentifiedObjects.Contains(identifiedObjects[i]))
                    {
                        if (OnBeamExit != null)
                            OnBeamExit(this, identifiedObjects[i]);

                        identifiedObjects.Remove(identifiedObjects[i]);
                    }
                }
            }
        }

        flagInitialized = true;
    }

    void InternalUpdate2DObjects()
    {
        if (_2DObjList.Length > 0)
        {
            if (pCheckChange.Length != _2DObjList.Length)
            {
                flagMeshUpdate = true;
                pCheckChange = new float[_2DObjList.Length];
            }

            Vector3 tPos = transform.position;
            float tRotZ = transform.rotation.z;
            for (int c = 0; c < _2DObjList.Length; c++)
            {
                float test = Vector3.SqrMagnitude(_2DObjList[c].transform.position - tPos) + _2DObjList[c].transform.rotation.z + tRotZ;

                if (test != pCheckChange[c])
                {
                    pCheckChange[c] = test;
                    flagMeshUpdate = true;
                }
            }
        }

        if (kColCount != _2DObjList.Length)
            flagMeshUpdate = true;
        kColCount = _2DObjList.Length;
    }

    void InternalUpdate3DObjects()
    {
        if (_3DObjList.Length > 0)
        {
            if (pCheckChange.Length != _3DObjList.Length)
            {
                flagMeshUpdate = true;
                pCheckChange = new float[_3DObjList.Length];
            }

            Vector3 tPos = transform.position;
            float tRotZ = transform.rotation.z;
            for (int c = 0; c < _3DObjList.Length; c++)
            {
                float test = Vector3.SqrMagnitude(_3DObjList[c].transform.position - tPos) + _3DObjList[c].transform.rotation.z + tRotZ;

                if (test != pCheckChange[c])
                {
                    pCheckChange[c] = test;
                    flagMeshUpdate = true;
                }
            }
        }

        if (kColCount != _3DObjList.Length)
            flagMeshUpdate = true;
        kColCount = _3DObjList.Length;
    }

    void InternalUpdateMesh()
    {
        Draw();

        _mesh.Clear();
        _mesh.vertices = verts.ToArray();

        UpdateTriangles();
        UpdateUVs();

        if (colors.Count != verts.Count)
            InternalUpdateColors();

        if (normals.Count != verts.Count)
            InternalUpdateNormals();

        _mesh.triangles = tris.ToArray();
        _mesh.uv = uvs.ToArray();
        _mesh.colors32 = colors.ToArray();
        _mesh.normals = normals.ToArray();
        _mesh.RecalculateBounds();

        if (!Application.isPlaying)
            _filter.sharedMesh = _mesh;
        else
            _filter.mesh = _mesh;

        flagMeshUpdate = false;
    }

    void InternalUpdateNormals()
    {
        normals.Clear();

        for (int i = 0; i < verts.Count; i++)
            normals.Add(-Vector3.forward);

        _mesh.normals = normals.ToArray();
        flagNormalsUpdate = false;
    }

    void InternalUpdateColors()
    {
        colors.Clear();

        for (int i = 0; i < verts.Count; i++)
            colors.Add(lightColor);

		if (_mesh.vertexCount != colors.Count)
			InternalUpdateMesh();

        _mesh.colors32 = colors.ToArray();
    }

    void InternalUpdateMaterial()
    {
        _renderer.material = lightMaterial;
    }

    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for those unfamiliar with Quaternion math as
    /// without that math its nearly impossible to get the right results using the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="_target">The GameObject you want the light to look at.</param>
    public void LookAt(GameObject _target)
    {
        LookAt(_target.transform.position);
    }
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for those unfamiliar with Quaternion math as
    /// without that math its nearly impossible to get the right results using the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="_target">The Transform you want the light to look at.</param>
    public void LookAt(Transform _target)
    {
        LookAt(_target.position);
    }
    /// <summary>
    /// A custom 'LookAt' funtion which looks along the lights 'Right' direction. This function was implimented for those unfamiliar with Quaternion math as
    /// without that math its nearly impossible to get the right results using the typical 'transform.LookAt' function.
    /// </summary>
    /// <param name="_target">The Vecto3 position you want the light to look at.</param>
    public void LookAt(Vector3 _target)
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _target, Vector3.forward) * lookAtRotation;
    }

    /// <summary>
    /// Toggles the light on or off
    /// </summary>
    /// <param name="_updateMesh">If 'TRUE' mesh will be forced to update. Use this if your light is dynamic when toggling it on.</param>
    /// <returns>'TRUE' if light is on.</returns>
    public bool ToggleLight(bool _updateMesh = false)
    {
        lightEnabled = !lightEnabled;
        return lightEnabled;
    }

    /// <summary>
    /// Provides and easy way to register your event method. The delegate takes the form of 'Foo(Light2D, GameObject)'.
    /// </summary>
    /// <param name="_eventType">Choose from 3 event types. 'OnEnter', 'OnStay', or 'OnExit'. Does not accept flags as argument.</param>
    /// <param name="_eventMethod">A callback method in the form of 'Foo(Light2D, GameObject)'.</param>
    public static void RegisterEventListener(LightEventListenerType _eventType, Light2DEvent _eventMethod)
    {
        if (_eventType == LightEventListenerType.OnEnter)
            OnBeamEnter += _eventMethod;

        if (_eventType == LightEventListenerType.OnStay)
            OnBeamStay += _eventMethod;

        if (_eventType == LightEventListenerType.OnExit)
            OnBeamExit += _eventMethod;
    }

    /// <summary>
    /// Provides and easy way to unregister your events. Usually used in the 'OnDestroy' and 'OnDisable' functions of your gameobject.
    /// </summary>
    /// <param name="_eventType">Choose from 3 event types. 'OnEnter', 'OnStay', or 'OnExit'. Does not accept flags as argument.</param>
    /// <param name="_eventMethod">The callback method you wish to remove.</param>
    public static void UnregisterEventListener(LightEventListenerType _eventType, Light2DEvent _eventMethod)
    {
        if (_eventType == LightEventListenerType.OnEnter)
            OnBeamEnter -= _eventMethod;

        if (_eventType == LightEventListenerType.OnStay)
            OnBeamStay -= _eventMethod;

        if (_eventType == LightEventListenerType.OnExit)
            OnBeamExit -= _eventMethod;
    }

    public void TriggerBeamEvent(LightEventListenerType eventType, GameObject eventGameObject)
    {
        switch (eventType)
        {
            case LightEventListenerType.OnEnter:
                if (OnBeamEnter != null)
                    OnBeamEnter(this, eventGameObject);
                break;

            case LightEventListenerType.OnStay:
                if (OnBeamStay != null)
                    OnBeamStay(this, eventGameObject);
                break;

            case LightEventListenerType.OnExit:
                if (OnBeamExit != null)
                    OnBeamExit(this, eventGameObject);
                break;
        }
    }
}