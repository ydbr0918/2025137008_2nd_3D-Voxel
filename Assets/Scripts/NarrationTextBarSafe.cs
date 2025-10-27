using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NarrationTextBarSafe : MonoBehaviour
{
    [Header("UI (���� ĳ���� ���â ����)")]
    public CanvasGroup barGroup;   // �ؽ�Ʈ �� �г�(CanvasGroup)
    public TMP_Text message;       // TMP �ؽ�Ʈ
    public Image portrait;         // ĳ���� ������(Image)

    [Header("Narration Icon Style")]
    public bool hidePortraitForNarration = true;   // �����̼� �� ������ ����
    [Range(0f, 1f)] public float narrationPortraitAlpha = 0f; // ������ ���� �� ����

    [Header("Fade")]
    public float fadeSeconds = 0.2f;

    [Header("Mode")]
    public bool timedMode = true;  // true: ���� �ð� �� �ڵ� ����, false: Sticky(���� ����)
    public float holdSeconds = 1.5f;

    Coroutine co;

    // �� Awake/Start/OnEnable ���� �ƹ� �͵� ���� �ʽ��ϴ�.
    // => ������ �� �ִ� "ó�� ���"�� ���� �ǵ帮�� ����

    public void ShowCharacter(string line, Sprite icon = null, float? seconds = null)
    {
        // ������ ����(������ ���� ���� ����)
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
        // �ؽ�Ʈ�� �� ����� ���� �ʴٸ� �ּ� ó�� ����
        if (message) message.text = "";

        // ���� ĳ���� ��� ��� ������ ����(��������Ʈ�� ������ ���̱�)
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

        // Fade In (���� ���Ŀ��� 1����)
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

        // Sticky ���� ���⼭ �� (����)
        if (!timedMode)
        {
            co = null;
            yield break;
        }

        // Timed ���� hold �� �ڵ� ����
        float hold = seconds.HasValue ? seconds.Value : holdSeconds;
        if (hold > 0f) yield return new WaitForSeconds(hold);

        // Fade Out (���� ���Ŀ��� 0����)
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

        // �ʿ� �� �ؽ�Ʈ ����(��ġ ������ �ּ�)
        if (message) message.text = "";

        // ������ ����
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
