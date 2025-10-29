using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public float lifeSeconds = 5f;
    public float damage = 10f; // 필요하면 플레이어에 데미지 적용

    void Start()
    {
        Destroy(gameObject, lifeSeconds);
    }

    void OnCollisionEnter(Collision other)
    {
        // 플레이어에 맞았을 때 처리(플레이어에 Health스크립트가 있다면 호출)
        // var hp = other.collider.GetComponent<PlayerHealth>();
        // if (hp) hp.TakeDamage(damage);

        // 충돌 시 파티클/사운드 넣을 수 있음
        Destroy(gameObject);
    }
}
