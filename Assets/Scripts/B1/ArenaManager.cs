using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // optional

public class ArenaManager : MonoBehaviour
{
    [Header("Setup")]
    public CannonController[] cannons;   // 씬에 배치한 5개 cannon
    public BossController boss;
    public float surviveSeconds = 30f;

    [Header("Overheat timing")]
    public float overheatToBossDelay = 0.8f; // 포대 과열 후 보스 폭발까지 대기

    [Header("UI (optional)")]
    public TMP_Text timerText;

    [Header("Events")]
    public UnityEvent onClear; // 클리어 시 호출(씬 전환 등)

    float t;

    void Start()
    {
        t = 0f;
        if (cannons == null || cannons.Length == 0) Debug.LogWarning("No cannons assigned");
        StartCoroutine(ArenaLoop());
    }

    IEnumerator ArenaLoop()
    {
        while (t < surviveSeconds)
        {
            t += Time.deltaTime;
            UpdateTimerUI(surviveSeconds - t);
            yield return null;
        }

        // 시간 종료 → 포대 과열
        TriggerOverheat();
        yield return new WaitForSeconds(overheatToBossDelay);

        // 보스 폭발
        if (boss != null)
        {
            boss.ExplodeAndClear();
        }

        // 클리어 콜백
        onClear?.Invoke();
    }

    void UpdateTimerUI(float remain)
    {
        if (timerText) timerText.text = Mathf.CeilToInt(remain).ToString();
    }

    void TriggerOverheat()
    {
        foreach (var c in cannons) if (c != null) c.StartOverheat();
    }

    // 디버그: 강제 오버히트(인스펙터 버튼으로 호출 가능)
    [ContextMenu("ForceOverheat")]
    public void ForceOverheat() => TriggerOverheat();
}
