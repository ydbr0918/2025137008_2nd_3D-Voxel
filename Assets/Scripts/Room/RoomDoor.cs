using System.Collections;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Who/Where")]
    public string playerTag = "Player";
    public Transform targetSpawn;          // ������ ���� ����(�ٴڿ� �μ���)

    [Header("Facing on Arrival")]
    public FacingMode facingMode = FacingMode.FaceAwayFromDoor; // �⺻: ���� ����
    public float forwardOffset = 0.25f;    // ���� �������� �������� ���� �о� �浹 ����

    [Header("Safety")]
    public float cooldown = 0.5f;          // �� ƨ��/�ߺ� Ʈ���� ����
    public float fadeSeconds = 0.25f;      // 0�̸� ���̵� �� ��

    float lastTime;

    public enum FacingMode
    {
        UseTargetForward,     // targetSpawn�� �ٶ󺸴� �������� ����
        FaceAwayFromDoor,     // �� ��(door)�� forward �������� ����(���� ����)
        FaceTowardDoor        // ���� �ٶ󺸰� ����(������ �ݴ�)
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
        // (����) ���̵� �ƿ�
        var cg = FindObjectOfType<CanvasGroup>(); // Overlay CanvasGroup�� �����صθ� ���ƿ�
        if (cg && fadeSeconds > 0f) yield return Fade(cg, 0f, 1f, fadeSeconds);

        // ���ϴ� ȸ�� ���(�ٴڸ�(Y�� ����))
        Quaternion rot = ComputeArrivalRotation();

        // ��ġ/ȸ�� ����
        Vector3 pos = targetSpawn ? targetSpawn.position : transform.position;
        pos += ProjectOnPlane(rot * Vector3.forward, Vector3.up) * forwardOffset; // ������ �� ��¦ ������

        // ���� ����: Rigidbody�� �ִٸ� �ӵ� 0
        var rb = player.GetComponent<Rigidbody>();
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        // ���� + Ʈ������ ����ȭ
        player.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();

        // (����) �� Ȱ��ȭ ����
        FindObjectOfType<RoomManager>()?.OnEnterRoom(targetSpawn ? targetSpawn.parent?.name : null);

        // �ڷ���Ʈ ���� F �ܻ� �Է� ���� (�ִٸ�)
        var prompt = FindObjectOfType<ElevatorAimPromptAndTalk>();
        prompt?.BlockInputForTeleport();

        // (����) ���̵� ��
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

        // �ٴڿ� ������ X/Z ������ ����
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
        // ���� ����/���� ����ȭ
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
