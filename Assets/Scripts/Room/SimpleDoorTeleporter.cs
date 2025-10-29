using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleDoorTeleporter : MonoBehaviour
{
    [Header("Who/Where")]
    public string playerTag = "Player";
    public Transform targetSpawn;                   // 목적지 스폰(위치/회전 참고)

    [Header("Facing on Arrival")]
    public FacingMode facingMode = FacingMode.FaceAwayFromDoor;
    [Tooltip("도착 지점에서 앞쪽으로 살짝 띄워 충돌 방지")]
    public float forwardOffset = 0.25f;

    [Header("Safety")]
    [Tooltip("반복 트리거 방지 쿨다운")]
    public float cooldown = 0.5f;
    [Tooltip("0이면 페이드 없음")]
    public float fadeSeconds = 0.25f;

    [Header("Optional Integrations")]
    [Tooltip("텔레포트 직후 F 오입력 방지(엘리베이터 프롬프트 사용 시 편함)")]
    public bool blockElevatorPrompt = true;

    float lastTime = -999f;

    public enum FacingMode
    {
        FaceTargetForward,   // ✅ 스폰 오브젝트의 forward로 선다(스폰 forward를 '문 바깥쪽'으로 두면 문을 등짐)
        FaceAwayFromDoor,    // 이 문(트리거) 오브젝트의 forward 쪽을 바라봄(문을 등짐 용도로 배치)
        FaceTowardDoor,      // 문을 바라보게 섬
        UseTargetForward     // (이전 호환) FaceTargetForward와 동일
    }

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; // 트리거 권장
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - lastTime < cooldown) return;
        if (!targetSpawn)
        {
            Debug.LogWarning($"[{name}] targetSpawn이 비어 있습니다.");
            return;
        }

        lastTime = Time.time;
        StartCoroutine(Teleport(other.transform));
    }

    IEnumerator Teleport(Transform player)
    {
        // (선택) 페이드 아웃
        var cg = FindObjectOfType<CanvasGroup>(); // 프로젝트에 맞게 전용 참조로 바꿔도 OK
        if (cg && fadeSeconds > 0f) yield return Fade(cg, 0f, 1f, fadeSeconds);

        // 도착 회전 계산 + 살짝 앞으로 밀기
        Quaternion rot = ComputeArrivalRotation();
        Vector3 pos = targetSpawn.position;
        pos += ProjectOnPlane(rot * Vector3.forward, Vector3.up) * forwardOffset;

        // 물리 안전: RB 있으면 속도 0
        var rb = player.GetComponent<Rigidbody>();
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        // 위치/회전 적용
        player.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();

        // (선택) 프롬프트 F 잔상 입력 방지
        if (blockElevatorPrompt)
            FindObjectOfType<ElevatorAimPromptAndTalk>()?.BlockInputForTeleport();

        // (선택) 페이드 인
        if (cg && fadeSeconds > 0f) yield return Fade(cg, 1f, 0f, fadeSeconds);
    }

    Quaternion ComputeArrivalRotation()
    {
        Vector3 dir;

        switch (facingMode)
        {
            case FacingMode.FaceTargetForward:   // ✅ 새 옵션
            case FacingMode.UseTargetForward:    // (구 옵션 호환)
                dir = targetSpawn ? targetSpawn.forward : transform.forward;
                break;

            case FacingMode.FaceAwayFromDoor:
                dir = transform.forward;
                break;

            default: // FacingMode.FaceTowardDoor
                dir = -transform.forward;
                break;
        }

        // 바닥면에 투영해 기울임 제거
        dir = ProjectOnPlane(dir, Vector3.up).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;

        return Quaternion.LookRotation(dir, Vector3.up);
    }

    static Vector3 ProjectOnPlane(Vector3 v, Vector3 n) => v - Vector3.Dot(v, n) * n;

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
        Gizmos.color = Color.green;
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
