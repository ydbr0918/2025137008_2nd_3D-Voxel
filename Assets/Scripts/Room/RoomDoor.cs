// RoomDoor.cs (교체본: '램프 검사' 추가)
using System.Collections;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Who/Where")]
    public string playerTag = "Player";
    public Transform targetSpawn;          // 옆방 스폰

    [Header("Facing on Arrival")]
    public FacingMode facingMode = FacingMode.FaceAwayFromDoor;
    public float forwardOffset = 0.25f;

    [Header("Safety")]
    public float cooldown = 0.5f;
    public float fadeSeconds = 0.25f;
    float lastTime;

    [Header("Lock by Lamp")]
    public bool requireLampOn = true;                // ← 램프 필요 여부
    public IndicatorLamp gateLamp;                   // ← 이 램프가 켜져 있어야 통과
    public NarrationTextBarSafe textBar;             // ← (선택) 잠김 안내 문구
    public string lockedText = "전원이 부족해 문이 열리지 않는다.";
    public float lockedNudgeBack = 0.2f;             // 잠김 때 살짝 밀어내기
    public AudioSource sfx;                          // (선택) 잠김 사운드
    public AudioClip lockedClip;                     // (선택)

    public enum FacingMode
    {
        UseTargetForward,
        FaceAwayFromDoor,
        FaceTowardDoor
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - lastTime < cooldown) return;

        // 🔒 램프 검사
        if (requireLampOn)
        {
            bool ok = (gateLamp != null && gateLamp.IsOn);
            if (!ok)
            {
                // 잠김 피드백
                if (textBar) textBar.ShowNarration(lockedText, 1.2f);
                if (sfx && lockedClip) sfx.PlayOneShot(lockedClip);

                // 플레이어를 문에서 살짝 뒤로 밀어서 '막힌 느낌'
                Vector3 pushDir = -transform.forward; pushDir.y = 0f; pushDir.Normalize();
                var tr = other.transform;
                tr.position += pushDir * lockedNudgeBack;
                Physics.SyncTransforms();

                lastTime = Time.time; // 연타 방지
                return;               // ❌ 텔레포트 금지
            }
        }

        // ✅ 통과 가능
        lastTime = Time.time;
        StartCoroutine(Teleport(other.transform));
    }

    IEnumerator Teleport(Transform player)
    {
        var cg = FindObjectOfType<CanvasGroup>();
        if (cg && fadeSeconds > 0f) yield return Fade(cg, 0f, 1f, fadeSeconds);

        Quaternion rot = ComputeArrivalRotation();
        Vector3 pos = targetSpawn ? targetSpawn.position : transform.position;
        pos += ProjectOnPlane(rot * Vector3.forward, Vector3.up) * forwardOffset;

        var rb = player.GetComponent<Rigidbody>();
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        player.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();

        FindObjectOfType<RoomManager>()?.OnEnterRoom(targetSpawn ? targetSpawn.parent?.name : null);

        var prompt = FindObjectOfType<ElevatorAimPromptAndTalk>();
        prompt?.BlockInputForTeleport();

        if (cg && fadeSeconds > 0f) yield return Fade(cg, 1f, 0f, fadeSeconds);
    }

    Quaternion ComputeArrivalRotation()
    {
        Vector3 dir = facingMode switch
        {
            FacingMode.UseTargetForward => (targetSpawn ? targetSpawn.forward : transform.forward),
            FacingMode.FaceAwayFromDoor => transform.forward,
            _ => -transform.forward
        };
        dir = ProjectOnPlane(dir, Vector3.up).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
        return Quaternion.LookRotation(dir, Vector3.up);
    }

    static Vector3 ProjectOnPlane(Vector3 v, Vector3 n) => v - Vector3.Dot(v, n) * n;

    IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        float t = 0f;
        while (t < d) { t += Time.deltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
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
