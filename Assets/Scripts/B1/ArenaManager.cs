using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro; // optional

public class ArenaManager : MonoBehaviour
{
    [Header("Setup")]
    public CannonController[] cannons;   // ���� ��ġ�� 5�� cannon
    public BossController boss;
    public float surviveSeconds = 30f;

    [Header("Overheat timing")]
    public float overheatToBossDelay = 0.8f; // ���� ���� �� ���� ���߱��� ���

    [Header("UI (optional)")]
    public TMP_Text timerText;

    [Header("Events")]
    public UnityEvent onClear; // Ŭ���� �� ȣ��(�� ��ȯ ��)

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

        // �ð� ���� �� ���� ����
        TriggerOverheat();
        yield return new WaitForSeconds(overheatToBossDelay);

        // ���� ����
        if (boss != null)
        {
            boss.ExplodeAndClear();
        }

        // Ŭ���� �ݹ�
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

    // �����: ���� ������Ʈ(�ν����� ��ư���� ȣ�� ����)
    [ContextMenu("ForceOverheat")]
    public void ForceOverheat() => TriggerOverheat();
}
