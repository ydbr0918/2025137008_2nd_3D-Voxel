using UnityEngine;

public class BulletRaycastProjectile : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;
    public int damage = 1;
    public LayerMask hitMask = ~0;
    public float hitRadius = 0f;
    public bool destroyOnHit = true;

    Vector3 prevPos;
    float t;

    void OnEnable()
    {
        prevPos = transform.position;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        Vector3 nextPos = transform.position + transform.forward * speed * dt;

        Vector3 dir = nextPos - prevPos;
        float dist = dir.magnitude;
        if (dist > 0f)
        {
            RaycastHit hit;
            bool hasHit = hitRadius > 0f
                ? Physics.SphereCast(prevPos, hitRadius, dir.normalized, out hit, dist, hitMask, QueryTriggerInteraction.Ignore)
                : Physics.Raycast(prevPos, dir.normalized, out hit, dist, hitMask, QueryTriggerInteraction.Ignore);

            if (hasHit)
            {
                var mh = hit.collider.GetComponentInParent<MonsterHealth>();
                if (mh != null) mh.TakeDamage(damage);
                if (destroyOnHit) { Destroy(gameObject); return; }
            }
        }

        transform.position = nextPos;
        prevPos = nextPos;

        t += dt;
        if (t >= lifeTime) Destroy(gameObject);
    }
}
