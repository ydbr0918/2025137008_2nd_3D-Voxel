using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public float lifeTime = 3f;
    public int damage = 10;
    public LayerMask hitMask;

    private float timer;
    private Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    public void Fire(Vector3 dir, float overrideSpeed = -1f)
    {
        float spd = overrideSpeed > 0 ? overrideSpeed : speed;
        rb.velocity = dir.normalized * spd;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifeTime) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // 여기에 데미지 처리 추가 가능
        // other.GetComponent<Enemy>()?.TakeDamage(damage);

        Destroy(gameObject);
    }
}
