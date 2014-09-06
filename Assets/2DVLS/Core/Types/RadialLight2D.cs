using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadialLight2D : Light2D 
{
    private static Dictionary<LightDetailSetting, Vector2[]> circleReferences = new Dictionary<LightDetailSetting, Vector2[]>();

    /// <summary>Sets the Radius of the light. Value clamped between 0.001f and Mathf.Infinity</summary>
    public float LightRadius { get { return lightRadius; } set { lightRadius = Mathf.Clamp(value, 0.001f, Mathf.Infinity); flagMeshUpdate = true; } }
    /// <summary>Sets the light cone starting point. Value 0 = Aims Right, Value 90 = Aims Up.</summary>
    public float LightConeStart { get { return coneStart; } set { coneStart = value; flagMeshUpdate = true; } }
    /// <summary>Sets the light cone size (wedge shape). Value is clamped between 0 and 360.</summary>
    public float LightConeAngle { get { return coneAngle; } set { coneAngle = Mathf.Clamp(value, 0f, 360f); flagMeshUpdate = true; } }

    public float UVRotation { get { return uvRotation; } set { uvRotation = value; UpdateUVs(); } }


    [SerializeField]
    private float lightRadius = 1;
    [SerializeField]
    private float coneStart = 0;
    [SerializeField]
    private float coneAngle = 360f;
    [SerializeField]
    private float uvRotation = 0;

    private bool coneEdgeGenerated = false;
    private float coneRangeMin = 0;
    private float coneRangeMax = 360;

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Light.png", false);
    }

    protected override void Initialize()
    {
        lookAtRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * Quaternion.Euler(-90, 0, 0);

        if (lightMaterial == null)
            lightMaterial = Resources.Load<Material>("RadialLight");
    }

    protected override void CollectColliders()
    {
        _2DObjList = Physics2D.OverlapAreaAll(transform.position + new Vector3(-lightRadius, lightRadius, 0), transform.position + new Vector3(lightRadius, -lightRadius, 0), shadowLayer);
    }

    protected override void Draw()
    {
        RaycastHit2D rhit2D = new RaycastHit2D();
        Vector2[] circleRef = GetCircleRef(lightDetail);

        bool wasHit = true;
        coneEdgeGenerated = false;
        int rays = (int)lightDetail;

        verts.Clear();

        if (coneAngle > 0)
        {
            verts.Add(Vector3.zero);
            UpdateConeMinMax();

            for (int i = 0; i < rays + 1; i++)
            {
                float a = i * (360f / (float)rays);

                if (coneAngle == 360 || (a >= coneRangeMin && a < coneRangeMax))
                {
                    rhit2D = Physics2D.Raycast(transform.position, transform.TransformDirection(Quaternion.Euler(0, 0, coneStart) * circleRef[i]), lightRadius, shadowLayer);

                    if (rhit2D.collider != null)
                    {
                        if (Application.isPlaying && useEvents && !unidentifiedObjects.Contains(rhit2D.transform.gameObject))
                            unidentifiedObjects.Add(rhit2D.transform.gameObject);

                        if (!wasHit)
                            verts.Add(Quaternion.Euler(0, 0, coneStart) * (circleRef[i - 1] * lightRadius));

                        float penetration = ((rhit2D.fraction * lightRadius) <= 0) ? 0 : lightPenetration;
                        Vector3 fPos = (rhit2D.point + (Vector2)circleRef[i] * penetration);
                        fPos = new Vector3(fPos.x, fPos.y, transform.position.z);
                        verts.Add(transform.InverseTransformPoint(fPos));

                        // Kill Vert Overkill
                        if (lightPenetration == 0 && verts.Count > 2)
                        {
                            prevPoints[0] = verts[verts.Count - 3];
                            prevPoints[1] = verts[verts.Count - 2];
                            prevPoints[2] = verts[verts.Count - 1];

                            if (Vector3.SqrMagnitude((prevPoints[0] - prevPoints[1]).normalized - (prevPoints[1] - prevPoints[2]).normalized) <= 0.01f)
                                verts.RemoveAt(verts.Count - 2);
                        }

                        wasHit = true;
                    }
                    else
                    {
                        if (a != 0 && wasHit)
                            verts.Add(Quaternion.Euler(0, 0, coneStart) * (circleRef[i] * lightRadius));

                        if (a == 45 || a == 135 || a == 225 || a == 315)
                            verts.Add(Quaternion.Euler(0, 0, coneStart) * (circleRef[i] * lightRadius));

                        wasHit = false;
                    }
                }

                if (coneAngle != 360 && (a >= coneRangeMax && !coneEdgeGenerated))
                {
                    rhit2D = Physics2D.Raycast(transform.position, transform.TransformDirection(Quaternion.Euler(0, 0, coneStart) * circleRef[i]), lightRadius, shadowLayer);

                    if (rhit2D.collider != null)
                    {
                        if (Application.isPlaying && useEvents && !unidentifiedObjects.Contains(rhit2D.transform.gameObject))
                            unidentifiedObjects.Add(rhit2D.transform.gameObject);

                        if (!wasHit)
                            verts.Add(Quaternion.Euler(0, 0, coneStart) * (circleRef[i - 1] * lightRadius));

                        float penetration = ((rhit2D.fraction * lightRadius) <= 0) ? 0 : lightPenetration;
                        Vector3 fPos = (rhit2D.point + (Vector2)circleRef[i] * penetration);

                        verts.Add(transform.InverseTransformPoint(fPos));

                        if (lightPenetration == 0 && verts.Count > 2)
                        {
                            prevPoints[0] = verts[verts.Count - 3];
                            prevPoints[1] = verts[verts.Count - 2];
                            prevPoints[2] = verts[verts.Count - 1];

                            if (Vector3.SqrMagnitude((prevPoints[0] - prevPoints[1]).normalized - (prevPoints[1] - prevPoints[2]).normalized) <= 0.01f)
                                verts.RemoveAt(verts.Count - 2);
                        }

                        wasHit = true;
                    }
                    else
                    {
                        if (a != 0 && wasHit)
                            verts.Add(Quaternion.Euler(0, 0, coneStart) * (circleRef[i] * lightRadius));

                        verts.Add(Quaternion.Euler(0, 0, coneStart) * (circleRef[i] * lightRadius));
                        wasHit = false;
                    }
                    coneEdgeGenerated = true;
                }
            }
        }
    }

    protected override void UpdateTriangles()
    {
        tris.Clear();

        for (int v = 0; v < _mesh.vertexCount - 1; v++)
        {
            tris.Add(0);
            tris.Add(v + 1);
            tris.Add(v);
        }

        if (coneAngle == 360)
        {
            tris.Add(0);
            tris.Add(1);
            tris.Add(verts.Count - 1);
        }
    }

    protected override void UpdateUVs()
    {
        uvs.Clear();
        Quaternion uvRot = Quaternion.Euler(0, 0, uvRotation - coneStart);

        for (int i = 0; i < verts.Count; i++)
        {
            Vector2 uv = uvRot * new Vector2((verts[i].x * 0.5f) / lightRadius, (verts[i].y * 0.5f) / lightRadius);
            uvs.Add(new Vector2(uv.x + 0.5f, uv.y + 0.5f));
        }
    }

    void UpdateConeMinMax()
    {
        coneRangeMin = 0;
        coneRangeMax = 360;

        if (coneAngle != 360)
        {
            coneRangeMin = (360f - coneAngle) * 0.5f;
            coneRangeMax = 180 + (coneAngle * 0.5f);
        }
    }

    static Vector2[] GetCircleRef(LightDetailSetting _detail)
    {
        if (!circleReferences.ContainsKey(_detail))
        {
            float x = 0;
            float y = 0;
            Vector3 v = Vector3.zero;
            int rays = (int)_detail;

            //circleRef.Clear();
            Vector2[] circleRef = new Vector2[rays + 1];

            for (int i = 0; i < rays + 1; i++)
            {
                float a = i * (360f / (float)rays);
                Vector2 circle = new Vector2(Mathf.Sin(a * Mathf.Deg2Rad) / Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Cos(a * Mathf.Deg2Rad) / Mathf.Sin(a * Mathf.Deg2Rad));

                if (a >= 315 || a <= 45)    // RIGHT SIDE
                {
                    x = 1;
                    y = x * circle.x;
                }

                if (a > 45 && a < 135)      // TOP SIDE
                {
                    y = 1;
                    x = y * circle.y;
                }

                if (a >= 135 && a <= 225)   // LEFT SIDE
                {
                    x = -1;
                    y = x * circle.x;
                }

                if (a > 225 && a < 315)     // BOTTOM SIDE
                {
                    y = -1;
                    x = y * circle.y;
                }

                v = new Vector3(x, y, 0);
                circleRef[i] = -v;
            }

            circleReferences.Add(_detail, circleRef);
            return circleRef;
        }
        else
        {
            return circleReferences[_detail];
        }
    }

    public static RadialLight2D Create(Vector3 _position, Color _color, float _radius = 1, float _coneAngle = 360, float _coneStart = 0, Material _material = null)
    {
        GameObject obj = new GameObject("RadialLight");
        obj.transform.position = _position;

        RadialLight2D l = obj.AddComponent<RadialLight2D>();

        l.LightColor = _color;
        l.LightRadius = _radius;
        l.LightConeAngle = _coneAngle;
        l.LightConeStart = _coneStart;

        l.LightMaterial = _material;
        if (_material == null)
            l.LightMaterial = Resources.Load<Material>("RadialLight");

        return l;
    }
}
