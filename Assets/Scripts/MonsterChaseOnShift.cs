using UnityEngine;

public class MonsterChaseOnShift : MonoBehaviour
{
    public Transform player;
    public PlayerController playerController;

    public float detectRadius = 8f;
    public float normalSpeed = 3f;
    public float chaseSpeed = 6.5f;
    public float stopDistance = 1.2f;
    public bool chaseWhenPlayerRuns = true;

    void Update()
    {
        if (player == null) return;

        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        float dist = Vector3.Distance(transform.position, target);

        bool playerRunning = playerController != null && playerController.IsRunning;
        bool shouldChase = (chaseWhenPlayerRuns && playerRunning) || dist <= detectRadius;

        if (!shouldChase) return;

        if (dist > stopDistance)
        {
            transform.LookAt(target);
            float spd = playerRunning ? chaseSpeed : normalSpeed;
            transform.position += (target - transform.position).normalized * spd * Time.deltaTime;
        }
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
