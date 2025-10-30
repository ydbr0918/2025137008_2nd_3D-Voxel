using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FootRangeRingSafe : MonoBehaviour
{
    public float radius = 8f;
    public float yOffset = 0.02f;
    public int segments = 64;
    public float width = 0.05f;
    public Material ringMaterial;
    public Color color = new Color(1, 0, 0, 0.7f);

    LineRenderer lr;
    Vector3[] pts;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = Mathf.Max(8, segments);
        lr.startWidth = width;
        lr.endWidth = width;
        if (ringMaterial != null) lr.material = ringMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        Build();
    }

    void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!lr) return;
        lr.positionCount = Mathf.Max(8, segments);
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;
        Build();
    }

    void Build()
    {
        int n = Mathf.Max(8, segments);
        if (pts == null || pts.Length != n) pts = new Vector3[n];
        float step = Mathf.PI * 2f / n;
        float y = yOffset;
        for (int i = 0; i < n; i++)
        {
            float a = step * i;
            pts[i] = new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius);
        }
        lr.SetPositions(pts);
    }
}
