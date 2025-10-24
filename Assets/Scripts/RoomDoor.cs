using UnityEditor.EditorTools;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform targetSpawn;     // ������ ���� ����
    public float cooldown = 0.5f;     // �� ƨ�� ����
    float lastTime;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - lastTime < cooldown) return;
        lastTime = Time.time;

        StartCoroutine(Teleport(other.transform));
    }

    System.Collections.IEnumerator Teleport(Transform player)
    {
        // (����) ���̵� �ƿ�
        var cg = FindObjectOfType<CanvasGroup>(); // ����� Overlay CanvasGroup ������ �ٲٸ� �� ����
        if (cg) yield return Fade(cg, 0f, 1f, 0.25f);

        // ��ġ �̵� + ī�޶� ����
        player.position = targetSpawn.position;
        player.rotation = targetSpawn.rotation;

        // (����) �� Ȱ��ȭ ����
        FindObjectOfType<RoomManager>()?.OnEnterRoom(targetSpawn.parent?.name);

        // (����) ���̵� ��
        if (cg) yield return Fade(cg, 1f, 0f, 0.25f);
    }

    System.Collections.IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        float t = 0f; while (t < d) { t += Time.deltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
    }
}
