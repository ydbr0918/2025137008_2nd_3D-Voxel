using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NarrationTextBarSafe : MonoBehaviour
{
    [Header("UI (기존 캐릭터 대사창 연결)")]
    public CanvasGroup barGroup;   // 텍스트 바 패널(CanvasGroup)
    public TMP_Text message;       // TMP 텍스트
    public Image portrait;         // 캐릭터 아이콘(Image)

    [Header("Narration Icon Style")]
    public bool hidePortraitForNarration = true;   // 내레이션 시 아이콘 숨김
    [Range(0f, 1f)] public float narrationPortraitAlpha = 0f; // 숨기지 않을 때 투명도

    [Header("Fade")]
    public float fadeSeconds = 0.2f;

    [Header("Mode")]
    public bool timedMode = true;  // true: 일정 시간 후 자동 숨김, false: Sticky(수동 숨김)
    public float holdSeconds = 1.5f;

    Coroutine co;

    // ★ Awake/Start/OnEnable 에서 아무 것도 하지 않습니다.
    // => 기존에 떠 있는 "처음 대사"를 절대 건드리지 않음

    public void ShowCharacter(string line, Sprite icon = null, float? seconds = null)
    {
        // 아이콘 세팅(없으면 기존 상태 유지)
        if (portrait)
        {
            if (icon) portrait.sprite = icon;
            if (portrait.sprite != null)
            {
                portrait.enabled = true;
                SetPortraitAlpha(1f);
            }
        }
        ShowLine(line, seconds);
    }

    public void ShowNarration(string line, float? seconds = null)
    {
        if (portrait)
        {
            if (hidePortraitForNarration)
            {
                portrait.enabled = false;
            }
            else
            {
                portrait.enabled = true;
                SetPortraitAlpha(narrationPortraitAlpha);
            }
        }
        ShowLine(line, seconds);
    }

    public void HideImmediate()
    {
        if (co != null) { StopCoroutine(co); co = null; }
        if (barGroup) { barGroup.alpha = 0f; barGroup.blocksRaycasts = false; }
        // 텍스트를 꼭 지우고 싶지 않다면 주석 처리 가능
        if (message) message.text = "";

        // 다음 캐릭터 대사 대비 아이콘 복원(스프라이트가 있으면 보이기)
        if (portrait)
        {
            portrait.enabled = (portrait.sprite != null);
            SetPortraitAlpha(1f);
        }
    }

    void ShowLine(string line, float? seconds)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowCo(line, seconds));
    }

    IEnumerator ShowCo(string line, float? seconds)
    {
        if (message) message.text = line;

        // Fade In (현재 알파에서 1까지)
        if (barGroup)
        {
            float a0 = barGroup.alpha;
            float t = 0f;
            barGroup.blocksRaycasts = false;
            while (t < fadeSeconds)
            {
                t += Time.deltaTime;
                barGroup.alpha = Mathf.Lerp(a0, 1f, t / fadeSeconds);
                yield return null;
            }
            barGroup.alpha = 1f;
        }

        // Sticky 모드면 여기서 끝 (유지)
        if (!timedMode)
        {
            co = null;
            yield break;
        }

        // Timed 모드면 hold 후 자동 숨김
        float hold = seconds.HasValue ? seconds.Value : holdSeconds;
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // Fade Out (현재 알파에서 0까지)
        if (barGroup)
        {
            float a0 = barGroup.alpha;
            float t = 0f;
            while (t < fadeSeconds)
            {
                t += Time.deltaTime;
                barGroup.alpha = Mathf.Lerp(a0, 0f, t / fadeSeconds);
                yield return null;
            }
            barGroup.alpha = 0f;
            barGroup.blocksRaycasts = false;
        }

        // 필요 시 텍스트 비우기(원치 않으면 주석)
        if (message) message.text = "";

        // 아이콘 복원
        if (portrait)
        {
            portrait.enabled = (portrait.sprite != null);
            SetPortraitAlpha(1f);
        }

        co = null;
    }

    void SetPortraitAlpha(float a)
    {
        if (!portrait) return;
        var c = portrait.color; c.a = a; portrait.color = c;
    }
}
