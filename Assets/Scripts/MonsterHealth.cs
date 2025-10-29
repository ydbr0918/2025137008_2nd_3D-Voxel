using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    public int maxHearts = 2;
    public string bulletTag = "Bullet";
    public bool destroyBulletOnHit = true;
    public HeartsUI heartsUI;

    int current;

    void Awake()
    {
        current = Mathf.Max(1, maxHearts);
        if (heartsUI == null) heartsUI = GetComponentInChildren<HeartsUI>(true);
        if (heartsUI != null) heartsUI.SetMaxAndCurrent(maxHearts, current);
    }

    public void TakeDamage(int amount = 1)
    {
        current = Mathf.Clamp(current - Mathf.Max(1, amount), 0, maxHearts);
        if (heartsUI != null) heartsUI.SetHearts(current);
        if (current <= 0) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bulletTag))
        {
            TakeDamage(1);
            if (destroyBulletOnHit) Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(bulletTag))
        {
            TakeDamage(1);
            if (destroyBulletOnHit) Destroy(collision.collider.gameObject);
        }
    }
}
