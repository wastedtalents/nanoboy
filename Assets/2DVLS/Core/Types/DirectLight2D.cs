using UnityEngine;
using System.Collections;

public class DirectLight2D : Light2D
{
    [SerializeField]
    private float beamSize = 25;
    [SerializeField]
    private float beamRange = 10;
    [SerializeField]
    private Vector2 uvTiling = new Vector2(1, 1);
    [SerializeField]
    private Vector2 uvOffset = new Vector2(0, 0);
    [SerializeField]
    private Vector3 pivotPoint = Vector3.zero;
    [SerializeField]
    private PivotPointType pivotPointType = PivotPointType.Center;

    /// <summary>Sets the size of the directional light in the X axis. Value clamped between 0.001f and Mathf.Infinity</summary>
    public float LightBeamSize { get { return beamSize; } set { beamSize = Mathf.Clamp(value, 0.001f, Mathf.Infinity); flagMeshUpdate = true; } }
    /// <summary>Sets the size of the directional light in the Y axis. Value clamped between 0.001f and Mathf.Infinity</summary>
    public float LightBeamRange { get { return beamRange; } set { beamRange = Mathf.Clamp(value, 0.001f, Mathf.Infinity); flagMeshUpdate = true; } }
    /// <summary>Returns the directional lights custom pivot point Vector.</summary>
    public Vector3 DiectionalLightPivotPoint
    {
        get
        {
            switch (pivotPointType)
            {
                case PivotPointType.Center:
                    return Vector3.zero;

                case PivotPointType.End:
                    return new Vector3(0, beamRange * -0.5f, 0);

                default:
                    return pivotPoint;
            }
        }
        set { pivotPoint = value; }
    }
    /// <summary>Sets which type of pivot point will be used on the directional light</summary>
    public PivotPointType DirectionalPivotPointType
    {
        get { return pivotPointType; }
        set { pivotPointType = value; flagMeshUpdate = true; }
    }
    /// <summary>Sets the UV tiling value</summary>
    public Vector2 UVTiling { get { return uvTiling; } set { uvTiling = value; flagMeshUpdate = true; } }
    /// <summary>Sets the UV offset value</summary>
    public Vector2 UVOffset { get { return uvOffset; } set { uvOffset = value; flagMeshUpdate = true; } }

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Directional.png", false);
    }

    protected override void Initialize()
    {
        lookAtRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * Quaternion.Euler(-90, 0, 90);
    }

    protected override void CollectColliders()
    {
        _2DObjList = Physics2D.OverlapAreaAll(transform.position - renderer.bounds.extents + DiectionalLightPivotPoint, transform.position + renderer.bounds.extents - DiectionalLightPivotPoint, shadowLayer); //Physics2D.OverlapAreaAll(transform.position + new Vector3(-lightRadius, lightRadius, 0), transform.position + new Vector3(lightRadius, -lightRadius, 0), shadowLayer);
    }

    protected override void Draw()
    {
        verts.Clear();

        if (_2DObjList.Length == 0)
        {
            verts.Add(DiectionalLightPivotPoint + new Vector3(-beamSize * 0.5f, -beamRange * 0.5f, 0));
            verts.Add(DiectionalLightPivotPoint + new Vector3(beamSize * 0.5f, -beamRange * 0.5f, 0));
            verts.Add(DiectionalLightPivotPoint + new Vector3(-beamSize * 0.5f, beamRange * 0.5f, 0));
            verts.Add(DiectionalLightPivotPoint + new Vector3(beamSize * 0.5f, beamRange * 0.5f, 0));
        }
        else
        {
            RaycastHit2D rhit2D = new RaycastHit2D();

            int rays = (int)lightDetail;
            bool wasHit = false;
            float spacing = beamSize / (rays - 1);

            for (int i = 0; i < rays; i++)
            {
                Vector3 rayStart = transform.TransformPoint(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * i), beamRange * 0.5f, 0));
                rhit2D = Physics2D.Raycast(rayStart, -transform.up, beamRange, shadowLayer);

                if (rhit2D.collider != null)
                {
                    if (Application.isPlaying && useEvents && !unidentifiedObjects.Contains(rhit2D.transform.gameObject))
                        unidentifiedObjects.Add(rhit2D.transform.gameObject);

                    if (!wasHit && i != 0)
                    {
                        verts.Add(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * i), beamRange * 0.5f, 0));
                        verts.Add(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * i), -beamRange * 0.5f, 0));
                    }

                    verts.Add(transform.InverseTransformPoint(rayStart));
                    verts.Add(transform.InverseTransformPoint(new Vector3(rhit2D.point.x, rhit2D.point.y, transform.position.z)));

                    if (i != (rays - 1) && verts.Count > 4)
                    {
                        prevPoints[0] = verts[verts.Count - 5];
                        prevPoints[1] = verts[verts.Count - 3];
                        prevPoints[2] = verts[verts.Count - 1];

                        if (Vector3.SqrMagnitude((prevPoints[0] - prevPoints[1]).normalized - (prevPoints[1] - prevPoints[2]).normalized) <= 0.01f)
                        {
                            verts.RemoveAt(verts.Count - 3);
                            verts.RemoveAt(verts.Count - 2);
                        }
                    }

                    wasHit = true;
                }
                else
                {
                    if (wasHit)
                    {
                        verts.Add(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * (i - 1)), beamRange * 0.5f, 0));
                        verts.Add(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * (i - 1)), -beamRange * 0.5f, 0));
                    }

                    if (i == 0 || i == (rays - 1))
                    {
                        verts.Add(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * i), beamRange * 0.5f, 0));
                        verts.Add(DiectionalLightPivotPoint + new Vector3((-beamSize * 0.5f) + (spacing * i), -beamRange * 0.5f, 0));
                    }

                    wasHit = false;
                }
            }
        }
    }

    protected override void UpdateTriangles()
    {
        tris.Clear();

        for (var v = 2; v < verts.Count - 1; v += 2)
        {
            tris.Add(v);
            tris.Add(v - 1);
            tris.Add(v - 2);

            tris.Add(v + 1);
            tris.Add(v - 1);
            tris.Add(v);
        }
    }

    protected override void UpdateUVs()
    {
        uvs.Clear();

        Vector2 dlp = (Vector2)DiectionalLightPivotPoint;

        for (int i = 0; i < verts.Count; i++)
        {
            uvs.Add(new Vector2((verts[i].x - dlp.x) / (beamSize * uvTiling.x) + (0.5f + uvOffset.x), (verts[i].y - dlp.y) / (beamRange * uvTiling.y) + (0.5f + uvOffset.y)));
        }
    }

    public static DirectLight2D Create(Vector3 _position, Color _color, float _beamSize = 10, float _beamRange = 1, Material _material = null)
    {
        GameObject obj = new GameObject("Radial Light2D");
        obj.transform.position = _position;

        DirectLight2D l = obj.AddComponent<DirectLight2D>();
        l.LightColor = _color;
        l.LightBeamSize = _beamSize;
        l.LightBeamRange = _beamRange;

        l.LightMaterial = _material;
        if (_material == null)
            l.LightMaterial = Resources.Load<Material>("DirectionalLight");

        return l;
    }
}
