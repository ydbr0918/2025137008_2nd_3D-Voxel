using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroLinesPlayer : MonoBehaviour
{
    [Header("UI References (�̹� ���� �ִ� �� ����)")]
    public CanvasGroup barGroup;     // �ؽ�Ʈ ��(�г�)�� CanvasGroup
    public TMP_Text message;       // �� ���� TMP �ؽ�Ʈ
    public Image portrait;      // �� ���� ������(Image). ������ ����α�

    [Header("Lines & Icons")]
    [TextArea] public string[] lines;     // ���ʴ�� ������ ����
    public Sprite defaultIcon;            // ���� ������(����)
    public Sprite[] lineIcons;            // �ٺ� ������(����, lines ���̿� ���߸� �� �ٿ��� ����)

    [Header("Text Settings")]
    public float fontSize = 36f;          // �ؽ�Ʈ ũ��
    public bool wordWrap = true;

    [Header("Typing Effect")]
    public bool typeEffect = true;       // Ÿ�� ȿ�� ��� ����
    public float charsPerSec = 40f;       // �ʴ� ���� ��

    [Header("Flow")]
    public bool autoStart = true;         // �������ڸ��� ���
    public bool clickToAdvance = true;    // Ŭ��/Ű�� ���� ��
    public KeyCode advanceKey = KeyCode.Mouse0; // �⺻: ���콺 ��Ŭ��
    public float fadeSeconds = 0.2f;      // �� ǥ��/���� ���̵�
    public UnityEvent onFinished;         // ������ ȣ��(���� �ܰ� Ʈ���� ��)

    int index = -1;            // ���� �� �ε���
    bool isTyping = false;     // Ÿ���� ��?
    string currentFullLine;    // ���� �� ��ü �ؽ�Ʈ
    Coroutine typingCo;

    void Start()
    {
        // �ʱ� ����
        if (message) { message.fontSize = fontSize; message.enableWordWrapping = wordWrap; message.text = ""; }
        if (barGroup) barGroup.alpha = 0f;

        if (autoStart) StartPlayback();
    }

    void Update()
    {
        if (!clickToAdvance) return;
        if (Input.GetKeyDown(advanceKey))
        {
            Proceed();
        }
    }

    // �ܺο��� ȣ���ؼ� ��� �����ϰ� ���� ��
    public void StartPlayback()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // �� ���̱�
        yield return Fade(barGroup, barGroup ? barGroup.alpha : 0f, 1f, fadeSeconds);
        ShowNextLine();
    }

    // Ŭ��/Ű �Է� �� ȣ��(��ư OnClick�� �����ص� ��)
    public void Proceed()
    {
        if (isTyping)
        {
            // Ÿ���� ���̸� ��� �ϼ�
            SkipTyping();
        }
        else
        {
            ShowNextLine();
        }
    }

    void ShowNextLine()
    {
        index++;

        if (index >= (lines?.Length ?? 0))
        {
            // ��
            StartCoroutine(FinishRoutine());
            return;
        }

        // ������ ����: �ٺ� ������ > �⺻ ������
        ApplyIconForIndex(index);

        currentFullLine = lines[index] ?? "";
        if (typeEffect) StartTyping(currentFullLine);
        else if (message) message.text = currentFullLine;
    }

    void ApplyIconForIndex(int i)
    {
        if (!portrait) return;

        Sprite use = null;
        if (lineIcons != null && i >= 0 && i < lineIcons.Length) use = lineIcons[i];
        if (!use) use = defaultIcon;

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

    IEnumerator TypeCo(string line)
    {
        isTyping = true;
        if (message) message.text = "";
        float t = 0f; int shown = 0;
        float cps = Mathf.Max(1f, charsPerSec);

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
        // �� ����
        yield return Fade(barGroup, 1f, 0f, fadeSeconds);
        onFinished?.Invoke();
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
