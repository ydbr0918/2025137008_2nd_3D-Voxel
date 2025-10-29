using UnityEngine;

public class MonsterChaser : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 10f;
    public float stopDistance = 1.5f;   // 플레이어에 너무 붙지 않도록
    public float loseDistance = 12f;    // 이 거리 밖이면 놓친 걸로 처리
    public float forgetAfter = 3f;      // 시야에서 벗어난 후 몇 초 뒤 추격 종료

    Transform target;       // 플레이어
    Vector3 homePos;        // 원래 자리(복귀용)
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
                // 나중에 순찰 넣고 싶으면 여기
                break;

            case State.Chase:
                if (target == null) { GoHome(); break; }

                float dist = Vector3.Distance(transform.position, target.position);
                if (dist <= loseDistance) lastSeenTime = Time.time;

                // 일정 시간 못봤으면 복귀
                if (Time.time - lastSeenTime > forgetAfter)
                {
                    GoHome();
                    break;
                }

                // 회전
                Vector3 dir = (target.position - transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion r = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, r, rotateSpeed * Time.deltaTime);
                }

                // 이동
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
        // Debug.Log($"{name} ALERT: chasing {player.name}");
    }

    void GoHome()
    {
        target = null;
        state = State.Return;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, loseDistance);
    }
#endif
}
