using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
//public enum ClearFlagsVLS
//{
//    Skybox = 1,
//    Color = 2
//    //Depth = 3,
//    //Nothing = 4
//}

[System.Serializable]
public class RenderPassVLS
{
    public bool applyLights = false;
    public LayerMask layerMask;
    //public ClearFlagsVLS clearFlags = ClearFlagsVLS.Color;
    public Color clearColor = Color.clear;

    [HideInInspector]
    public bool activeLayer = true;
    [HideInInspector]
    public RenderTexture rTexture = null;
}

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ProShader : MonoBehaviour 
{
    const string MultiplyShader = "2DVLS/Multiply";
    const string BlurShader = "2DVLS/Blur";
    const string AlphaShader = "2DVLS/Alpha";

    public int iterations = 3;
    public float blurSpread = 0.6f;

    public bool useProjectAmbientColor = false;
    public LayerMask lightLayer;
    public Color ambientColor = new Color(0.025f, 0, 0.1f, 0.9f);

    [SerializeField]
    public RenderPassVLS[] renderPassList = new RenderPassVLS[] { new RenderPassVLS() { applyLights = true } };

    RenderTexture _Source;
    RenderTexture _LightBuffer;

    int _pixelWidth;
    int _pixelHeight;

    GameObject _renderCam;

    public Material _blendMat;
    public Material _blurMat;
    public Material _alphaMat;

    void OnEnable()
    {
        if (_blendMat == null)
            _blendMat = new Material(Shader.Find(MultiplyShader));

        if (_blurMat == null)
            _blurMat = new Material(Shader.Find(BlurShader));

        if (_alphaMat == null)
            _alphaMat = new Material(Shader.Find(AlphaShader));

        _renderCam = new GameObject("LightCam", typeof(Camera));
        _renderCam.camera.enabled = false;
        _renderCam.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnDisable()
    {
        DestroyImmediate(_renderCam);
    }

    void OnPostRender()
    {
        Camera cam = _renderCam.camera;
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.clear;
        cam.CopyFrom(camera);

        _pixelWidth = (int)camera.pixelWidth;
        _pixelHeight = (int)camera.pixelHeight;

        RenderLights(cam);

        foreach (RenderPassVLS rPass in renderPassList)
            RenderPass(rPass, cam);
    }
    
    void RenderLights(Camera _cam)
    {
        _LightBuffer = RenderTexture.GetTemporary(_pixelWidth, _pixelHeight, 0, RenderTextureFormat.ARGB32);
        _cam.backgroundColor = (useProjectAmbientColor) ? RenderSettings.ambientLight : ambientColor;
        _cam.cullingMask = lightLayer;
        _cam.targetTexture = _LightBuffer;
        _cam.Render();

        if(_blurMat != null)
            BlitBlurEffect(_LightBuffer, _LightBuffer, _blurMat);
    }

    void RenderPass(RenderPassVLS _passSettings, Camera _cam)
    {
        _passSettings.rTexture = RenderTexture.GetTemporary(_pixelWidth, _pixelHeight, 0, RenderTextureFormat.ARGB32);

        if (_passSettings.applyLights)
            _cam.clearFlags = CameraClearFlags.Color;
        //else
        //    _cam.clearFlags = (CameraClearFlags)_passSettings.clearFlags;

        _cam.backgroundColor = _passSettings.clearColor;
        _cam.cullingMask = _passSettings.layerMask;
        _cam.targetTexture = _passSettings.rTexture;
        _cam.Render();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        foreach (RenderPassVLS rPass in renderPassList)
        {
            if (!rPass.activeLayer)
                continue;

            _alphaMat.SetTexture("_Alpha", rPass.rTexture);

            if (rPass.applyLights)
                Graphics.Blit(_LightBuffer, rPass.rTexture, _blendMat);

            Graphics.Blit(rPass.rTexture, source, _alphaMat);
        }

        Graphics.Blit(source, destination);

        for (int i = renderPassList.Length - 1; i >= 0; i--)
            CleanTexture(renderPassList[i].rTexture);

        CleanTexture(_LightBuffer);
    }

    void CleanTexture(RenderTexture _rt)
    {
        RenderTexture.ReleaseTemporary(_rt);
    }

    public void BlitBlurEffect(RenderTexture source, RenderTexture destination, Material material)
    {
        int rtW = source.width / 4;
        int rtH = source.height / 4;

        RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

        // Copy source to the 4x4 smaller texture.
        DownSample4x(source, buffer, material);

        // Blur the small texture
        for (int i = 0; i < iterations; i++)
        {
            RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
            FourTapCone(buffer, buffer2, material, i);
            RenderTexture.ReleaseTemporary(buffer);
            buffer = buffer2;
        }

        Graphics.Blit(buffer, destination, material);

        RenderTexture.ReleaseTemporary(buffer);
    }

    // Performs one blur iteration.
    public void FourTapCone(RenderTexture source, RenderTexture dest, Material material, int iteration)
    {
        float off = 0.5f + iteration * blurSpread;
        Graphics.BlitMultiTap(source, dest, material,
            new Vector2(-off, -off),
            new Vector2(-off, off),
            new Vector2(off, off),
            new Vector2(off, -off)
        );
    }

    // Downsamples the texture to a quarter resolution.
    private void DownSample4x(RenderTexture source, RenderTexture dest, Material material)
    {
        float off = 1.0f;
        Graphics.BlitMultiTap(source, dest, material,
            new Vector2(-off, -off),
            new Vector2(-off, off),
            new Vector2(off, off),
            new Vector2(off, -off)
        );
    }
}
