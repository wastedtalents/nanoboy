using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shadow2D : Light2D
{
    private static Dictionary<LightDetailSetting, Vector2[]> circleReferences = new Dictionary<LightDetailSetting, Vector2[]>();

    /// <summary>Sets the Radius of the light. Value clamped between 0.001f and Mathf.Infinity</summary>
    public float LightRadius { get { return lightRadius; } set { lightRadius = Mathf.Clamp(value, 0.001f, Mathf.Infinity); flagMeshUpdate = true; } }
    /// <summary>Sets the light cone starting point. Value 0 = Aims Right, Value 90 = Aims Up.</summary>
    public float LightConeStart { get { return coneStart; } set { coneStart = value; flagMeshUpdate = true; } }
    /// <summary>Sets the light cone size (wedge shape). Value is clamped between 0 and 360.</summary>
    public float LightConeAngle { get { return coneAngle; } set { coneAngle = Mathf.Clamp(value, 0f, 360f); flagMeshUpdate = true; } }

    [SerializeField]
    private float lightRadius = 1;
    [SerializeField]
    private float coneStart = 0;
    [SerializeField]
    private float coneAngle = 360f;

    private bool coneEdgeGenerated = false;
    private float coneRangeMin = 0;
    private float coneRangeMax = 360;

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Shadow.png", false);
    }

    protected override void Initialize()
    {
        lookAtRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * Quaternion.Euler(-90, 0, 0);

        if (lightMaterial == null)
            lightMaterial = Resources.Load<Material>("Shadow");
    }

    protected override void CollectColliders()
    {
        _2DObjList = Physics2D.OverlapAreaAll(transform.position + new Vector3(-lightRadius, lightRadius, 0), transform.position + new Vector3(lightRadius, -lightRadius, 0), shadowLayer);
    }

    protected override void Draw()
    {
        RaycastHit2D rhit2D = new RaycastHit2D();
        Vector2[] circleRef = GetCircleRef(lightDetail);

        coneEdgeGenerated = false;
        int rays = (int)lightDetail;
        bool wasHitA = false;
        bool wasHitB = false;

        verts.Clear();
        tris.Clear();

        if (coneAngle > 0)
        {
            UpdateConeMinMax();
            Vector3 zOffset = new Vector3(0, 0, transform.position.z);

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

                        float penetration = ((rhit2D.fraction * lightRadius) <= 0) ? 0 : lightPenetration;
                        Vector3 fPos = (rhit2D.point + (Vector2)circleRef[i] * penetration);


                        verts.Add(transform.InverseTransformPoint(fPos));
                        verts.Add((Quaternion.Euler(0, 0, coneStart) * (circleRef[i] * lightRadius)) - zOffset);

                        if (verts.Count > 3 && wasHitA)
                        {
                            int v = verts.Count - 4;
                            tris.Add(v + 2);
                            tris.Add(v + 1);
                            tris.Add(v);

                            tris.Add(v + 2);
                            tris.Add(v + 3);
                            tris.Add(v + 1);
                        }

                        wasHitA = true;
                    }
                    else
                    {
                        wasHitA = false;
                    }
                }
                
                if (coneAngle != 360 && (a >= coneRangeMax && !coneEdgeGenerated))
                {
                    rhit2D = Physics2D.Raycast(transform.position, transform.TransformDirection(Quaternion.Euler(0, 0, coneStart) * circleRef[i]), lightRadius, shadowLayer);

                    if (rhit2D.collider != null)
                    {
                        if (Application.isPlaying && useEvents && !unidentifiedObjects.Contains(rhit2D.transform.gameObject))
                            unidentifiedObjects.Add(rhit2D.transform.gameObject);

                        float penetration = ((rhit2D.fraction * lightRadius) <= 0) ? 0 : lightPenetration;
                        Vector3 fPos = (rhit2D.point + (Vector2)circleRef[i] * penetration);

                        verts.Add(transform.InverseTransformPoint(fPos));
                        verts.Add((Quaternion.Euler(0, 0, coneStart) * (circleRef[i] * lightRadius)) - zOffset);

                        if (verts.Count > 3 && wasHitB)
                        {
                            int v = verts.Count - 4;
                            tris.Add(v + 2);
                            tris.Add(v + 1);
                            tris.Add(v);

                            tris.Add(v + 2);
                            tris.Add(v + 3);
                            tris.Add(v + 1);
                        }

                        wasHitB = true;
                    }
                    else
                    {
                        wasHitB = false;
                    }

                    coneEdgeGenerated = true;
                }
            }
        }
    }

    protected override void UpdateTriangles()
    { }

    protected override void UpdateUVs()
    {
        uvs.Clear();

        for (int i = 0; i < verts.Count; i++)
        {
            Vector2 uv = Quaternion.Euler(0, 0, -coneStart) * new Vector2((verts[i].x * 0.5f) / lightRadius, (verts[i].y * 0.5f) / lightRadius);
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

    public static Shadow2D Create(Vector3 _position, Color _color, float _radius = 1, float _coneAngle = 360, float _coneStart = 0, Material _material = null)
    {
        GameObject obj = new GameObject("Shadow Emitter");
        obj.transform.position = _position;

        Shadow2D l = obj.AddComponent<Shadow2D>();

        l.LightColor = _color;
        l.LightRadius = _radius;
        l.LightConeAngle = _coneAngle;
        l.LightConeStart = _coneStart;

        l.LightMaterial = _material;
        if (_material == null)
            l.LightMaterial = Resources.Load<Material>("Shadow");

        return l;
    }
}
