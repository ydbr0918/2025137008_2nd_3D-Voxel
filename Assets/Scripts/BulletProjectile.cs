using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 3f;
    public float hitDestroyDelay = 0f;

    Rigidbody rb;
    float t;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 dir, float forceMultiplier = 1f)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(dir.normalized * speed * forceMultiplier, ForceMode.VelocityChange);
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= lifeTime) Destroy(gameObject);
    }

    void OnCollisionEnter(Collision c)
    {
        Destroy(gameObject, hitDestroyDelay);
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject, hitDestroyDelay);
    }
}
