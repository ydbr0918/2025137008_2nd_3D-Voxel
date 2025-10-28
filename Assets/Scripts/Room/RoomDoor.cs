using System.Collections;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Who/Where")]
    public string playerTag = "Player";
    public Transform targetSpawn;          // 옆방의 스폰 지점(바닥에 두세요)

    [Header("Facing on Arrival")]
    public FacingMode facingMode = FacingMode.FaceAwayFromDoor; // 기본: 문을 등짐
    public float forwardOffset = 0.25f;    // 도착 지점에서 앞쪽으로 조금 밀어 충돌 방지

    [Header("Safety")]
    public float cooldown = 0.5f;          // 문 튕김/중복 트리거 방지
    public float fadeSeconds = 0.25f;      // 0이면 페이드 안 함

    float lastTime;

    public enum FacingMode
    {
        UseTargetForward,     // targetSpawn의 바라보는 방향으로 선다
        FaceAwayFromDoor,     // 이 문(door)의 forward 방향으로 선다(문을 등짐)
        FaceTowardDoor        // 문을 바라보게 선다(등짐의 반대)
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - lastTime < cooldown) return;
        lastTime = Time.time;

        StartCoroutine(Teleport(other.transform));
    }

    IEnumerator Teleport(Transform player)
    {
        // (선택) 페이드 아웃
        var cg = FindObjectOfType<CanvasGroup>(); // Overlay CanvasGroup을 연결해두면 좋아요
        if (cg && fadeSeconds > 0f) yield return Fade(cg, 0f, 1f, fadeSeconds);

        // 원하는 회전 계산(바닥면(Y만 유지))
        Quaternion rot = ComputeArrivalRotation();

        // 위치/회전 적용
        Vector3 pos = targetSpawn ? targetSpawn.position : transform.position;
        pos += ProjectOnPlane(rot * Vector3.forward, Vector3.up) * forwardOffset; // 문에서 한 발짝 앞으로

        // 물리 안전: Rigidbody가 있다면 속도 0
        var rb = player.GetComponent<Rigidbody>();
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        // 적용 + 트랜스폼 동기화
        player.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();

        // (선택) 방 활성화 관리
        FindObjectOfType<RoomManager>()?.OnEnterRoom(targetSpawn ? targetSpawn.parent?.name : null);

        // 텔레포트 직후 F 잔상 입력 무시 (있다면)
        var prompt = FindObjectOfType<ElevatorAimPromptAndTalk>();
        prompt?.BlockInputForTeleport();

        // (선택) 페이드 인
        if (cg && fadeSeconds > 0f) yield return Fade(cg, 1f, 0f, fadeSeconds);
    }

    Quaternion ComputeArrivalRotation()
    {
        Vector3 dir;

        switch (facingMode)
        {
            case FacingMode.UseTargetForward:
                dir = targetSpawn ? targetSpawn.forward : transform.forward;
                break;

            case FacingMode.FaceAwayFromDoor:
            
                dir = transform.forward;
                break;

            default: // FacingMode.FaceTowardDoor
                dir = -transform.forward;
                break;
        }

        // 바닥에 투영해 X/Z 기울어짐 제거
        dir = ProjectOnPlane(dir, Vector3.up).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;

        return Quaternion.LookRotation(dir, Vector3.up);
    }

    static Vector3 ProjectOnPlane(Vector3 v, Vector3 n)
    {
        return v - Vector3.Dot(v, n) * n;
    }

    IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        float t = 0f;
        while (t < d)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(a, b, t / d);
            yield return null;
        }
        g.alpha = b;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 도착 지점/방향 가시화
        Gizmos.color = Color.cyan;
        var rot = Application.isPlaying ? ComputeArrivalRotation()
                                        : (facingMode == FacingMode.FaceAwayFromDoor ? transform.rotation
                                                                                     : Quaternion.LookRotation(-transform.forward, Vector3.up));

        Vector3 p = targetSpawn ? targetSpawn.position : transform.position;
        Vector3 dir = ProjectOnPlane(rot * Vector3.forward, Vector3.up).normalized;

        Gizmos.DrawWireSphere(p, 0.1f);
        Gizmos.DrawLine(p, p + dir * 0.8f);
        Gizmos.DrawWireSphere(p + dir * forwardOffset, 0.06f);
    }
#endif
}
