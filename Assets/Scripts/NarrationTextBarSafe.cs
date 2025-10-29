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
    public bool hidePortraitForNarration = true;
    [Range(0f, 1f)] public float narrationPortraitAlpha = 0f;

    [Header("Fade")]
    public float fadeSeconds = 0.2f;

    [Header("Mode")]
    public bool timedMode = true;   // true: 일정 시간 후 자동 숨김, false: Sticky
    public float holdSeconds = 1.5f;

    [Header("Visibility Options")]
    public bool autoHideOnStart = false;   
    public bool deactivateOnHide = false;  
    Coroutine co;

    void Awake()
    {
        // 누락 참조 자동 보정
        if (!barGroup) barGroup = GetComponentInChildren<CanvasGroup>(true);
        if (!message) message = GetComponentInChildren<TMP_Text>(true);
        if (!portrait) portrait = GetComponentInChildren<Image>(true);
    }

    void Start()
    {
        // ★ 처음 대사를 보여야 한다면 이 옵션을 꺼두세요.
        if (autoHideOnStart)
        {
            HideImmediate();
        }
        else
        {
            // 시작 시 보이게 둘 때, 필요한 기본 플래그 정리
            if (barGroup)
            {
                barGroup.blocksRaycasts = (barGroup.alpha > 0f);
                barGroup.interactable = (barGroup.alpha > 0f);
            }
        }
    }

    public void ShowCharacter(string line, Sprite icon = null, float? seconds = null)
    {
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

        if (barGroup)
        {
            barGroup.alpha = 0f;
            barGroup.blocksRaycasts = false;
            barGroup.interactable = false;
        }
        if (message) message.text = "";

        if (portrait)
        {
            portrait.enabled = (portrait.sprite != null);
            SetPortraitAlpha(1f);
        }

        if (deactivateOnHide) gameObject.SetActive(false);
    }

    void ShowLine(string line, float? seconds)
    {
        if (deactivateOnHide && !gameObject.activeSelf)
            gameObject.SetActive(true);

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowCo(line, seconds));
    }

    IEnumerator ShowCo(string line, float? seconds)
    {
        if (message) message.text = line;

        // Fade In
        if (barGroup)
        {
            float a0 = barGroup.alpha;
            float t = 0f;

            barGroup.blocksRaycasts = true;
            barGroup.interactable = true;

            while (t < fadeSeconds)
            {
                t += Time.deltaTime;
                barGroup.alpha = Mathf.Lerp(a0, 1f, t / fadeSeconds);
                yield return null;
            }
            barGroup.alpha = 1f;
        }

        if (!timedMode)
        {
            co = null;
            yield break;
        }

        float hold = seconds.HasValue ? seconds.Value : holdSeconds;
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // Fade Out
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
            barGroup.interactable = false;
        }

        if (message) message.text = "";

        if (portrait)
        {
            portrait.enabled = (portrait.sprite != null);
            SetPortraitAlpha(1f);
        }

        if (deactivateOnHide) gameObject.SetActive(false);

        co = null;
    }

    void SetPortraitAlpha(float a)
    {
        if (!portrait) return;
        var c = portrait.color; c.a = a; portrait.color = c;
    }
}
