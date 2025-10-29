using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimingLockMinigame : MonoBehaviour
{
    [Header("Bar / Cursor")]
    public RectTransform barArea;     // ���� ��(�θ�)
    public RectTransform cursor;      // �����̴� �� ����

    [Header("Targets (order)")]
    public RectTransform[] targetZones;   // ������� 3��(Ȥ�� N��) Ÿ�� ����
    public Image[] targetFills;           // �� Ÿ���� �ð� �ǵ��(���󺯰�). targetZones�� ���� ����/����

    [Header("Motion")]
    public float speed = 1.6f;            // Ŀ�� �պ� �ӵ�(���)
    public bool startFromRandomSide = true;

    [Header("Input")]
    public KeyCode key = KeyCode.Space;   // �Ǵ� F

    [Header("Rules")]
    public bool resetOnFail = true;       // ���� �� ó������
    public Color idleColor = new Color(1, 1, 1, 0.15f);
    public Color successColor = new Color(0.2f, 1f, 0.2f, 0.8f);
    public Color failFlashColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    public float failFlashSeconds = 0.15f;

    [Header("SFX (optional)")]
    public AudioSource sfx;
    public AudioClip hitOk;
    public AudioClip hitFail;
    public AudioClip complete;

    [Header("Result")]
    public UnityEvent onUnlocked;         // ��� ���� �� 1ȸ ȣ��

    int currentIndex = 0;                 // ���� ����� �ϴ� Ÿ�� �ε���
    float t;                              // PingPong �ð�
    int dirSign = 1;                      // 1 �Ǵ� -1 : ���� ����
    bool isCompleted = false;
    float barWidth;

    void OnEnable()
    {
        if (barArea == null || cursor == null || targetZones == null || targetZones.Length == 0)
        {
            Debug.LogError("[TimingLockMinigame] ��/Ŀ��/Ÿ�� ������ Ȯ���ϼ���.");
            enabled = false;
            return;
        }

        // �ʱ� ����
        if (targetFills != null)
        {
            for (int i = 0; i < targetFills.Length; i++)
                if (targetFills[i]) targetFills[i].color = idleColor;
        }

        barWidth = ((RectTransform)barArea).rect.width;

        // ���� ����/��ġ ����
        if (startFromRandomSide)
        {
            dirSign = Random.value < 0.5f ? -1 : 1;
            t = Random.Range(0f, 1f);
        }
        else { dirSign = 1; t = 0f; }

        isCompleted = false;
        currentIndex = 0;
        PlaceCursor(0f);  // �ʱ� ��ġ
    }

    void Update()
    {
        if (isCompleted) return;

        // �¡�� ����
        t += Time.deltaTime * speed;
        float ping = Mathf.PingPong(t, 1f);      // 0~1
        float signed = dirSign > 0 ? ping : 1f - ping;

        PlaceCursor(signed);

        // �Է�
        if (Input.GetKeyDown(key) || Input.GetKeyDown(KeyCode.F))
        {
            bool ok = IsCursorInside(targetZones[currentIndex]);
            if (ok)
            {
                if (sfx && hitOk) sfx.PlayOneShot(hitOk);
                if (targetFills != null && currentIndex < targetFills.Length && targetFills[currentIndex])
                    targetFills[currentIndex].color = successColor;

                currentIndex++;

                if (currentIndex >= targetZones.Length)
                {
                    // �Ϸ�
                    isCompleted = true;
                    if (sfx && complete) sfx.PlayOneShot(complete);
                    onUnlocked?.Invoke();
                }
            }
            else
            {
                if (sfx && hitFail) sfx.PlayOneShot(hitFail);
                // ª�� ���� �÷���
                if (targetFills != null && currentIndex < targetFills.Length && targetFills[currentIndex])
                    StartCoroutine(FlashFail(targetFills[currentIndex]));

                if (resetOnFail)
                {
                    // ��ü �ʱ�ȭ
                    currentIndex = 0;
                    for (int i = 0; i < targetFills.Length; i++)
                        if (targetFills[i]) targetFills[i].color = idleColor;
                }
                // �ƴϸ� ���� �ε����� �ٽ� �õ�
            }
        }
    }

    System.Collections.IEnumerator FlashFail(Image img)
    {
        if (!img) yield break;
        Color prev = img.color;
        img.color = failFlashColor;
        yield return new WaitForSeconds(failFlashSeconds);
        img.color = prev;
    }

    void PlaceCursor(float normalized01)
    {
        // barArea ���� ���� 0~1 ��ġ�� Ŀ�� ��ġ
        var rt = cursor;
        float x = Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, normalized01);
        var p = rt.anchoredPosition;
        p.x = x;
        rt.anchoredPosition = p;
    }

    bool IsCursorInside(RectTransform zone)
    {
        // Ŀ���� Ÿ���� ���� X ���� �� (���� �θ� barArea ����)
        Vector2 cMin = GetRectLocalMin(cursor);
        Vector2 cMax = GetRectLocalMax(cursor);
        Vector2 zMin = GetRectLocalMin(zone);
        Vector2 zMax = GetRectLocalMax(zone);

        // X�� ��ħ üũ (Y�� ����, ���� ���ο� �δ� ����)
        return (cMin.x <= zMax.x) && (cMax.x >= zMin.x);
    }

    Vector2 GetRectLocalMin(RectTransform rt)
    {
        var r = rt.rect;
        var p = rt.anchoredPosition;
        return p + r.min;
    }
    Vector2 GetRectLocalMax(RectTransform rt)
    {
        var r = rt.rect;
        var p = rt.anchoredPosition;
        return p + r.max;
    }
}
