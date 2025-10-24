using UnityEditor.EditorTools;
using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform targetSpawn;     // 옆방의 스폰 지점
    public float cooldown = 0.5f;     // 문 튕김 방지
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
        // (선택) 페이드 아웃
        var cg = FindObjectOfType<CanvasGroup>(); // 당신의 Overlay CanvasGroup 참조로 바꾸면 더 좋음
        if (cg) yield return Fade(cg, 0f, 1f, 0.25f);

        // 위치 이동 + 카메라 보정
        player.position = targetSpawn.position;
        player.rotation = targetSpawn.rotation;

        // (선택) 방 활성화 관리
        FindObjectOfType<RoomManager>()?.OnEnterRoom(targetSpawn.parent?.name);

        // (선택) 페이드 인
        if (cg) yield return Fade(cg, 1f, 0f, 0.25f);
    }

    System.Collections.IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        float t = 0f; while (t < d) { t += Time.deltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
    }
}
