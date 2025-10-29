using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimingLockMinigame : MonoBehaviour
{
    [Header("Bar / Cursor")]
    public RectTransform barArea;     // 수평 바(부모)
    public RectTransform cursor;      // 움직이는 흰 막대

    [Header("Targets (order)")]
    public RectTransform[] targetZones;   // 순서대로 3개(혹은 N개) 타깃 영역
    public Image[] targetFills;           // 각 타깃의 시각 피드백(색상변경). targetZones와 같은 순서/개수

    [Header("Motion")]
    public float speed = 1.6f;            // 커서 왕복 속도(배수)
    public bool startFromRandomSide = true;

    [Header("Input")]
    public KeyCode key = KeyCode.Space;   // 또는 F

    [Header("Rules")]
    public bool resetOnFail = true;       // 실패 시 처음부터
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
    public UnityEvent onUnlocked;         // 모두 성공 시 1회 호출

    int currentIndex = 0;                 // 현재 노려야 하는 타깃 인덱스
    float t;                              // PingPong 시간
    int dirSign = 1;                      // 1 또는 -1 : 진행 방향
    bool isCompleted = false;
    float barWidth;

    void OnEnable()
    {
        if (barArea == null || cursor == null || targetZones == null || targetZones.Length == 0)
        {
            Debug.LogError("[TimingLockMinigame] 바/커서/타깃 설정을 확인하세요.");
            enabled = false;
            return;
        }

        // 초기 색상
        if (targetFills != null)
        {
            for (int i = 0; i < targetFills.Length; i++)
                if (targetFills[i]) targetFills[i].color = idleColor;
        }

        barWidth = ((RectTransform)barArea).rect.width;

        // 시작 방향/위치 랜덤
        if (startFromRandomSide)
        {
            dirSign = Random.value < 0.5f ? -1 : 1;
            t = Random.Range(0f, 1f);
        }
        else { dirSign = 1; t = 0f; }

        isCompleted = false;
        currentIndex = 0;
        PlaceCursor(0f);  // 초기 배치
    }

    void Update()
    {
        if (isCompleted) return;

        // 좌↔우 핑퐁
        t += Time.deltaTime * speed;
        float ping = Mathf.PingPong(t, 1f);      // 0~1
        float signed = dirSign > 0 ? ping : 1f - ping;

        PlaceCursor(signed);

        // 입력
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
                    // 완료
                    isCompleted = true;
                    if (sfx && complete) sfx.PlayOneShot(complete);
                    onUnlocked?.Invoke();
                }
            }
            else
            {
                if (sfx && hitFail) sfx.PlayOneShot(hitFail);
                // 짧은 실패 플래시
                if (targetFills != null && currentIndex < targetFills.Length && targetFills[currentIndex])
                    StartCoroutine(FlashFail(targetFills[currentIndex]));

                if (resetOnFail)
                {
                    // 전체 초기화
                    currentIndex = 0;
                    for (int i = 0; i < targetFills.Length; i++)
                        if (targetFills[i]) targetFills[i].color = idleColor;
                }
                // 아니면 같은 인덱스를 다시 시도
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
        // barArea 좌측 기준 0~1 위치로 커서 배치
        var rt = cursor;
        float x = Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, normalized01);
        var p = rt.anchoredPosition;
        p.x = x;
        rt.anchoredPosition = p;
    }

    bool IsCursorInside(RectTransform zone)
    {
        // 커서와 타깃의 로컬 X 범위 비교 (같은 부모 barArea 기준)
        Vector2 cMin = GetRectLocalMin(cursor);
        Vector2 cMax = GetRectLocalMax(cursor);
        Vector2 zMin = GetRectLocalMin(zone);
        Vector2 zMax = GetRectLocalMax(zone);

        // X축 겹침 체크 (Y는 무시, 같은 라인에 두는 전제)
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
