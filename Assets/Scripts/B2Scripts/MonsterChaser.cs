using UnityEngine;

public class MonsterChaser : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 10f;
    public float stopDistance = 1.5f;
    public float loseDistance = 12f;
    public float forgetAfter = 3f;
    public string playerTag = "Player";

    Transform target;
    Vector3 homePos;
    float lastSeenTime;

    enum State { Idle, Chase, Return }
    State state = State.Idle;

    void Awake()
    {
        homePos = transform.position;
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;

            case State.Chase:
                if (target == null) { GoHome(); break; }

                float dist = Vector3.Distance(transform.position, target.position);
                if (dist <= loseDistance) lastSeenTime = Time.time;

                if (Time.time - lastSeenTime > forgetAfter)
                {
                    GoHome();
                    break;
                }

                Vector3 dir = target.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion r = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, r, rotateSpeed * Time.deltaTime);
                }

                if (dist > stopDistance)
                {
                    transform.position += transform.forward * (moveSpeed * Time.deltaTime);
                }
                break;

            case State.Return:
                Vector3 toHome = homePos - transform.position;
                toHome.y = 0f;
                if (toHome.magnitude < 0.05f)
                {
                    transform.position = new Vector3(homePos.x, transform.position.y, homePos.z);
                    state = State.Idle;
                    break;
                }

                if (toHome.sqrMagnitude > 0.001f)
                {
                    Quaternion r2 = Quaternion.LookRotation(toHome, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, r2, rotateSpeed * Time.deltaTime);
                    transform.position += transform.forward * (moveSpeed * Time.deltaTime);
                }
                break;
        }
    }

    public void Alert(Transform player)
    {
        target = player;
        state = State.Chase;
        lastSeenTime = Time.time;
    }

    void GoHome()
    {
        target = null;
        state = State.Return;
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

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, loseDistance);
    }
#endif
}
