using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class MonsterChase1 : MonoBehaviour
{
    public Transform player;
    public float detectRadius = 8f;
    public float moveSpeed = 3f;
    public float stopDistance = 1.2f;
    public float ringYOffset = 0.02f;
    public int ringSegments = 64;
    public float ringWidth = 0.05f;

    LineRenderer lr;
    Rigidbody rb;
    CapsuleCollider col;
    Vector3[] points;

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
        lr.positionCount = ringSegments;
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        if (lr.sharedMaterial == null)
        {
            var mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = new Color(1, 0, 0, 0.7f);
            lr.material = mat;
        }
        points = new Vector3[ringSegments];
        BuildRing();
    }

    void BuildRing()
    {
        float step = Mathf.PI * 2f / ringSegments;
        float y = ringYOffset;
        for (int i = 0; i < ringSegments; i++)
        {
            float a = step * i;
            float x = Mathf.Cos(a) * detectRadius;
            float z = Mathf.Sin(a) * detectRadius;
            points[i] = new Vector3(x, y, z);
        }
        lr.SetPositions(points);
    }

    void OnValidate()
    {
        if (ringSegments < 8) ringSegments = 8;
        if (Application.isPlaying && lr != null)
        {
            lr.positionCount = ringSegments;
            points = new Vector3[ringSegments];
            BuildRing();
        }
    }

    void Update()
    {
        if (player == null) return;
        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        float dist = Vector3.Distance(transform.position, target);
        if (dist <= detectRadius && dist > stopDistance)
        {
            transform.LookAt(target);
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }
    }
}


