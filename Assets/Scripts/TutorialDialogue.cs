using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup barGroup;      // TutorialBottomBar�� CanvasGroup
    public Image icon;                // ����
    public TMP_Text message;          // TMP �ؽ�Ʈ

    [Header("Texts")]
    [TextArea] public string firstLine = "Ʃ�丮�� �°� ȯ����.";
    [TextArea] public string secondLine = "������ �÷����ϱ����� �����ϰ� ���۹��� �˾ƺ���.";

    [Header("Typing")]
    public bool typeEffect = true;
    public float charsPerSec = 40f;

    [Header("Fade Out On Finish")]
    public float fadeOutSeconds = 0.35f;

    // �ܺο��� ���� ����(���â�� ������ ����� �� ȣ��)
    public event Action OnDialogueFinished;

    // 0: ù ����, 1: �� ��° ����, 2: ���� ���, 3: ��
    int state = 0;
    bool inputLocked = false; // ����Ŭ���� ��ŵ�Ǵ� �� ����

    void OnEnable()
    {
        // ù �� ǥ��
        ShowLine(firstLine);
    }

    void Update()
    {
        if (state >= 3 || inputLocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Proceed();
        }
    }

    public void Proceed() // ��ư OnClick���ε� ȣ�� ����
    {
        if (state >= 3 || inputLocked) return;

        if (state == 0)
        {
            // ù ���� ���� Ŭ��: ��� �ϼ�, �̹� �ϼ��̸� ���� ��
            if (typeEffect && message.text != firstLine) { StopAllCoroutines(); message.text = firstLine; }
            else { state = 1; ShowLine(secondLine); }
        }
        else if (state == 1)
        {
            if (typeEffect && message.text != secondLine) { StopAllCoroutines(); message.text = secondLine; }
            else
            {
                state = 2;
                StartCoroutine(FadeOutAndClose());
            }
        }
    }

    void ShowLine(string line)
    {
        if (!message) return;

        if (typeEffect)
        {
            StopAllCoroutines();
            StartCoroutine(TypeCo(line));
        }
        else message.text = line;
    }

    IEnumerator TypeCo(string line)
    {
        message.text = "";
        float t = 0f; int shown = 0; float cps = Mathf.Max(1f, charsPerSec);
        while (shown < line.Length)
        {
            t += Time.deltaTime * cps;
            int target = Mathf.Clamp(Mathf.FloorToInt(t), 0, line.Length);
            if (target != shown) { shown = target; message.text = line.Substring(0, shown); }
            yield return null;
        }
        message.text = line;
    }

    IEnumerator FadeOutAndClose()
    {
        inputLocked = true; // ���� Ŭ�� ����

        if (barGroup)
        {
            barGroup.interactable = false; // Ŭ�� ����
            barGroup.blocksRaycasts = false;

            float t = 0f, a0 = barGroup.alpha;
            while (t < fadeOutSeconds)
            {
                t += Time.deltaTime;
                barGroup.alpha = Mathf.Lerp(a0, 0f, t / fadeOutSeconds);
                yield return null;
            }
            barGroup.alpha = 0f;
        }

        // �� ��ü ��Ȱ��ȭ(���ϸ� Destroy�� �ٲ㵵 ��)
        gameObject.SetActive(false);

        state = 3;
        inputLocked = false;
        OnDialogueFinished?.Invoke(); // ���� �ý���(Ʃ�丮�� �ܰ�) ���� ��ȣ
    }
}
