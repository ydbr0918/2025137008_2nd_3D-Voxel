using UnityEngine;
using System.Collections.Generic;

public class CctvZoneDetectorSimple : MonoBehaviour
{
    [Header("Blink")]
    public float activeTime = 1f;
    public float inactiveTime = 1f;

    [Header("Area")]
    public float radius = 3f;
    public float height = 2f;
    public bool frontOnly = true;

    [Header("Visual")]
    public Renderer zoneRenderer;
    public Color activeColor = new Color(1, 0, 0, 0.5f);
    public Color inactiveColor = new Color(1, 0, 0, 0.1f);

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Monster Alert")]
    public float alertRadius = 20f;
    public LayerMask monsterMask = ~0;  // 물리 검색시만 사용
    public float alertCooldown = 1.0f;
    public bool usePhysicsQuery = false; // ← false면 트랜스폼 거리로 검색(권장)
    public bool debugLogs = true;

    bool isActive;
    float timer;
    Transform player;
    float nextAlertTime = 0f;

    void Start()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go) player = go.transform;
        SetActive(false);

        if (!zoneRenderer)
        {
            zoneRenderer = GetComponentInChildren<Renderer>();
        }
    }

    void Update()
    {
        // 깜빡임
        timer += Time.deltaTime;
        if (isActive && timer >= activeTime) SetActive(false);
        else if (!isActive && timer >= inactiveTime) SetActive(true);

        if (!isActive || !player) return;

        // 감지 판정(반경/높이/반원)
        Vector3 p = transform.position;
        Vector3 a = player.position;
        if (Mathf.Abs(a.y - p.y) > height * 0.5f) return;

        Vector3 d = a - p; d.y = 0f;
        if (d.magnitude > radius) return;

        if (frontOnly)
        {
            Vector3 f = transform.forward; f.y = 0f; f.Normalize();
            if (Vector3.Dot(f, d.normalized) < 0f) return;
        }

        TryAlertMonsters();
    }

    void TryAlertMonsters()
    {
        if (Time.time < nextAlertTime) return;
        nextAlertTime = Time.time + alertCooldown;

        int count = 0;

        if (usePhysicsQuery)
        {
            // 콜라이더 가진 오브젝트만 찾음
            var hits = Physics.OverlapSphere(transform.position, alertRadius, monsterMask, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                var m = h.GetComponentInParent<MonsterChaser>() ?? h.GetComponent<MonsterChaser>();
                if (m) { m.Alert(player); count++; }
            }
        }
        else
        {
            // 콜라이더/레이어 상관없이 트랜스폼 거리로 탐지 (권장)
            var all = FindObjectsOfType<MonsterChaser>();
            float r2 = alertRadius * alertRadius;
            foreach (var m in all)
            {
                if (!m.enabled) continue;
                if ((m.transform.position - transform.position).sqrMagnitude <= r2)
                {
                    m.Alert(player);
                    count++;
                }
            }
        }

        if (debugLogs) Debug.Log($"[CCTV:{name}] alerted {count} monsters.");
    }

    void SetActive(bool state)
    {
        isActive = state;
        timer = 0f;
        if (zoneRenderer)
        {
            var mat = zoneRenderer.material;
            mat.color = isActive ? activeColor : inactiveColor;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActive ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, alertRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * radius);
    }
#endif
}
