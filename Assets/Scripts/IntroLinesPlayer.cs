using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroLinesPlayer : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup barGroup;
    public TMP_Text message;
    public Image portrait;

    [Header("Lines & Icons")]
    [TextArea] public string[] lines;
    public Sprite defaultIcon;
    public Sprite[] lineIcons;

    [Header("Text Settings")]
    public float fontSize = 36f;
    public bool wordWrap = true;

    [Header("Typing")]
    public bool typeEffect = true;
    public float charsPerSec = 40f;

    [Header("Flow")]
    public bool autoStart = true;
    public bool clickToAdvance = true;     // Button�� ���� false�� �ΰ� Button.onClick�� Proceed()�� ����
    public KeyCode advanceKey = KeyCode.Mouse0;
    public float fadeSeconds = 0.2f;
    public UnityEvent onFinished;
    public bool disableAfterFinish = true; // ������ ������Ʈ ��Ȱ��

    int index = -1;
    bool isTyping = false;
    bool finished = false;
    string currentFullLine;
    Coroutine typingCo;

    // Ŭ�� ��Ÿ/�������� �ߺ� ó�� ����
    float nextAcceptTime = 0f;
    public float clickCooldown = 0.12f; // 120ms

    public bool deactivateBarOnFinish = true;  // �� GameObject ��Ȱ��
    public bool destroyComponentOnFinish = true; // ������Ʈ ����(����� ��õ ����)

    void Start()
    {
        if (message) { message.fontSize = fontSize; message.enableWordWrapping = wordWrap; message.text = ""; }
        if (barGroup) { barGroup.alpha = 0f; barGroup.blocksRaycasts = false; }
        if (autoStart) StartPlayback();
    }

    void Update()
    {
        if (!clickToAdvance) return;
        if (finished) return;
        if (Time.time < nextAcceptTime) return;

        if (Input.GetKeyDown(advanceKey))
        {
            nextAcceptTime = Time.time + clickCooldown;
            Proceed();
        }
    }

    public void StartPlayback()
    {
        if (finished) return; // �̹� ���� ���¸� ����� ����(���ϸ� ResetPlayback() ���� ���� ��)
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // �� ���̱�
        if (barGroup) { barGroup.blocksRaycasts = true; yield return Fade(barGroup, barGroup.alpha, 1f, fadeSeconds); }
        index = -1;
        ShowNextLine();
    }

    public void Proceed()
    {
        if (finished) return;

        if (isTyping) SkipTyping();
        else ShowNextLine();
    }

    void ShowNextLine()
    {
        index++;

        if (index >= (lines?.Length ?? 0))
        {
            StartCoroutine(FinishRoutine());
            return;
        }

        ApplyIconForIndex(index);
        currentFullLine = lines[index] ?? "";

        if (typeEffect) StartTyping(currentFullLine);
        else if (message) message.text = currentFullLine;
    }

    void ApplyIconForIndex(int i)
    {
        if (!portrait) return;
        Sprite use = (lineIcons != null && i >= 0 && i < lineIcons.Length && lineIcons[i] != null)
            ? lineIcons[i]
            : defaultIcon;

        portrait.sprite = use;
        portrait.enabled = (use != null);
    }

    void StartTyping(string line)
    {
        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = StartCoroutine(TypeCo(line));
    }

    void SkipTyping()
    {
        if (typingCo != null) StopCoroutine(typingCo);
        isTyping = false;
        if (message) message.text = currentFullLine;
    }

    void OnDisable()
    {
        // Ȥ�� ��Ȱ��/Ȱ�� ����Ŭ���� �ٽ� �������� �� ����
        if (barGroup) { barGroup.alpha = 0f; barGroup.blocksRaycasts = false; }
        if (message) message.text = string.Empty;
    }

    IEnumerator TypeCo(string line)
    {
        isTyping = true;
        if (message) message.text = "";
        float t = 0f; int shown = 0; float cps = Mathf.Max(1f, charsPerSec);

        while (shown < line.Length)
        {
            t += Time.deltaTime * cps;
            int target = Mathf.Clamp(Mathf.FloorToInt(t), 0, line.Length);
            if (target != shown)
            {
                shown = target;
                if (message) message.text = line.Substring(0, shown);
            }
            yield return null;
        }
        if (message) message.text = line;
        isTyping = false;
    }

    IEnumerator FinishRoutine()
    {
        finished = true;

        // �Է� ���� ����
        if (barGroup)
        {
            barGroup.blocksRaycasts = false;
            yield return Fade(barGroup, 1f, 0f, fadeSeconds);
        }

        // �ؽ�Ʈ ���� ����
        if (message) message.text = string.Empty;

        // �� ��ü�� ��Ȱ��(���ϸ� �ı�)
        if (deactivateBarOnFinish && barGroup)
            barGroup.gameObject.SetActive(false);

        onFinished?.Invoke();

        // ������Ʈ/������Ʈ ����(�ߺ� ��� ����)
        if (destroyComponentOnFinish)
            Destroy(this);                // ĵ������ ���� ���� ������Ʈ�� ����
                                          // ���� ���� ������Ʈ��� �Ʒ��� ��ü ����:
                                          // Destroy(gameObject);
    }

    IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        if (!g) yield break;
        float t = 0f; g.alpha = a;
        while (t < d)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(a, b, t / d);
            yield return null;
        }
        g.alpha = b;
    }
}
