using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class MonsterChase1 : MonoBehaviour
{
    public Transform player;
    public string playerTag = "Player";
    public float detectRadius = 8f;
    public float moveSpeed = 3f;
    public float stopDistance = 1.2f;
    public float ringYOffset = 0.02f;
    public int ringSegments = 64;
    public float ringWidth = 0.05f;
    public Material ringMaterial;
    public Color ringColor = new Color(1, 0, 0, 0.7f);
    public float reacquireInterval = 1f;

    LineRenderer lr;
    Rigidbody rb;
    CapsuleCollider col;
    Vector3[] points;
    float nextReacquire;
    float prevRadius, prevYOffset, prevWidth;
    int prevSegments;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.isKinematic = true;
        rb.useGravity = false;
        col.isTrigger = true;

        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = Mathf.Max(8, ringSegments);
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        if (ringMaterial != null) lr.material = ringMaterial;
        lr.startColor = ringColor;
        lr.endColor = ringColor;

        points = new Vector3[lr.positionCount];
        BuildRing();

        if (player == null) TryAcquirePlayer();
        CacheRingParams();
    }

    void Update()
    {
        if (player == null && Time.time >= nextReacquire) TryAcquirePlayer();

        if (player != null)
        {
            Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
            float dist = Vector3.Distance(transform.position, target);
            if (dist <= detectRadius && dist > stopDistance)
            {
                transform.LookAt(target);
                Vector3 dir = (target - transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
        }

        if (RingParamsChanged()) { ApplyRingParams(); BuildRing(); }
    }

    void TryAcquirePlayer()
    {
        var go = GameObject.FindWithTag(playerTag);
        player = go ? go.transform : null;
        nextReacquire = Time.time + reacquireInterval;
    }

    void BuildRing()
    {
        int n = Mathf.Max(8, ringSegments);
        if (lr.positionCount != n) lr.positionCount = n;
        if (points == null || points.Length != n) points = new Vector3[n];

        float step = Mathf.PI * 2f / n;
        float y = ringYOffset;
        for (int i = 0; i < n; i++)
        {
            float a = step * i;
            float x = Mathf.Cos(a) * detectRadius;
            float z = Mathf.Sin(a) * detectRadius;
            points[i] = new Vector3(x, y, z);
        }
        lr.SetPositions(points);
    }

    void ApplyRingParams()
    {
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.startColor = ringColor;
        lr.endColor = ringColor;
        if (ringMaterial != null && lr.sharedMaterial != ringMaterial) lr.material = ringMaterial;
        CacheRingParams();
    }

    void CacheRingParams()
    {
        prevRadius = detectRadius;
        prevYOffset = ringYOffset;
        prevWidth = ringWidth;
        prevSegments = ringSegments;
    }

    bool RingParamsChanged()
    {
        return prevRadius != detectRadius || prevYOffset != ringYOffset || prevWidth != ringWidth || prevSegments != ringSegments;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawn.Respawn(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerRespawn.Respawn(collision.collider.gameObject);
        }
    }
}
