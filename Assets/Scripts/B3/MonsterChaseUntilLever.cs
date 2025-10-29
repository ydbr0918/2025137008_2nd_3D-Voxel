using UnityEngine;

public class MonsterChaseUntilLever : MonoBehaviour
{
    public Transform player;
    public LeverCloseDoor lever;
    public float detectRadius = 8f;
    public float moveSpeed = 3.5f;
    public float stopDistance = 1.2f;

    void Update()
    {
        if (lever != null && lever.Activated) return;
        if (player == null) return;

        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        float dist = Vector3.Distance(transform.position, target);
        if (dist > detectRadius) return;

        if (dist > stopDistance)
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
            GameReset.ReloadCurrentScene();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            GameReset.ReloadCurrentScene();
        }
    }
}
