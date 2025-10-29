using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public float lifeSeconds = 5f;
    public float damage = 10f; // �ʿ��ϸ� �÷��̾ ������ ����

    void Start()
    {
        Destroy(gameObject, lifeSeconds);
    }

    void OnCollisionEnter(Collision other)
    {
        // �÷��̾ �¾��� �� ó��(�÷��̾ Health��ũ��Ʈ�� �ִٸ� ȣ��)
        // var hp = other.collider.GetComponent<PlayerHealth>();
        // if (hp) hp.TakeDamage(damage);

        // �浹 �� ��ƼŬ/���� ���� �� ����
        Destroy(gameObject);
    }
}
