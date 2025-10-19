using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialSequence : MonoBehaviour
{
    [Header("UI refs")]
    public CanvasGroup barGroup;      // TutorialBottomBar�� CanvasGroup
    public Image icon;                // ����
    public TMP_Text message;          // TMP �ؽ�Ʈ

    [Header("Typing")]
    public bool useTyping = true;
    public float charsPerSec = 40f;

    [Header("Fade")]
    public float fadeSeconds = 0.35f;

    [System.Serializable]
    public class Step
    {
        [TextArea] public string line;      // ���ܿ��� ǥ���� ����
        public Sprite lineIcon;             // ���� ������
        public Condition condition;         // � ������ �����ϸ� ���� �Ϸ�?
        public bool hideBarWhileWaiting;    // �� ���ܿ����� �ٸ� ���� ä�� ���?
        public float distanceNeeded = 2.0f; // Move���� ���� �̵� �Ÿ� ����
        public int clicksNeeded = 3;        // Shoot �� Ŭ�� Ƚ�� ����
        [TextArea] public string successLine; // ���� �޼� �� ������ �����߾�!�� ���� (����)
        public bool waitForClickAfterSuccess; // ���� ���� �� Ŭ�� ��ٸ���(�� ���� �������� ��� �Ѿ��)
    }

    public enum Condition
    {
        ClickToContinue,  // Ŭ���ϸ� ����
        MoveWASD,         // WASD �̵� ����
        SwitchViewV,      // VŰ ���� ���� ����
        AimRMB,           // ��Ŭ�� ���� ����
        ShootLMB,         // ��Ŭ�� nȸ
        InteractE,        // E ��ȣ�ۿ� 1ȸ
        None              // �ٷ� ����(������)
    }

    [Header("Steps (�������)")]
    public Step[] steps;

    // ���� ����
    int i = -1;
    float moved;
    float aimHold;
    int clicks;
    bool interacted;
    bool viewSwitched;
    bool running;
    bool typing;

    void Start()
    {
        // ���� �� ù ��������
        NextStep();
    }

    void Update()
    {
        if (!running) return;
        var s = steps[i];

        // �� ���� ������Ʈ
        switch (s.condition)
        {
            case Condition.MoveWASD:
                UpdateMoveAccumulate();
                if (moved >= s.distanceNeeded) StartCoroutine(CompleteStep(s));
                break;

            case Condition.SwitchViewV:
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current?.vKey.wasPressedThisFrame == true)
                    viewSwitched = true;
#else
                if (Input.GetKeyDown(KeyCode.V)) viewSwitched = true;
#endif
                if (viewSwitched) StartCoroutine(CompleteStep(s));
                break;

            case Condition.AimRMB:
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Mouse.current?.rightButton.isPressed == true) aimHold += Time.deltaTime;
#else
                if (Input.GetMouseButton(1)) aimHold += Time.deltaTime;
#endif
                if (aimHold >= 0.8f) StartCoroutine(CompleteStep(s));
                break;

            case Condition.ShootLMB:
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Mouse.current?.leftButton.wasPressedThisFrame == true) clicks++;
#else
                if (Input.GetMouseButtonDown(0)) clicks++;
#endif
                if (clicks >= s.clicksNeeded) StartCoroutine(CompleteStep(s));
                break;

            case Condition.InteractE:
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current?.eKey.wasPressedThisFrame == true) interacted = true;
#else
                if (Input.GetKeyDown(KeyCode.E)) interacted = true;
#endif
                if (interacted) StartCoroutine(CompleteStep(s));
                break;

            case Condition.ClickToContinue:
                if (Input.GetMouseButtonDown(0)) StartCoroutine(CompleteStep(s));
                break;

            case Condition.None:
                StartCoroutine(CompleteStep(s));
                break;
        }
    }

    void UpdateMoveAccumulate()
    {
        // Player ��ġ ����(���� Player �±װ� �ִٰ� ����)
        var p = GameObject.FindGameObjectWithTag("Player");
        if (!p) return;
        if (!_lastValid) { _last = p.transform.position; _lastValid = true; return; }
        var cur = p.transform.position;
        moved += Vector3.Distance(cur, _last);
        _last = cur;
    }
    Vector3 _last; bool _lastValid;

    IEnumerator CompleteStep(Step s)
    {
        running = false;

        // ���� ������ �ְ�, �ٸ� �����ָ� ���� �ǵ���� �ְ� �ʹٸ�
        if (!string.IsNullOrEmpty(s.successLine))
        {
            // �ٰ� ������ �ִ� �����̸� ���� ��Ÿ����
            if (s.hideBarWhileWaiting) yield return StartCoroutine(FadeBar(1f));

            yield return ShowText(s.successLine, s.lineIcon);

            if (s.waitForClickAfterSuccess)
            {
                // Ŭ���� ��ٷȴٰ� ��������
                while (!Input.GetMouseButtonDown(0)) yield return null;
            }
        }

        // ���� ��������
        NextStep();
    }

    void NextStep()
    {
        // ���� ����
        moved = 0f; aimHold = 0f; clicks = 0; interacted = false; viewSwitched = false; _lastValid = false;

        i++;
        if (i >= steps.Length)
        {
            // ��ü Ʃ�丮�� ����: �� ����
            StartCoroutine(FadeBar(0f));
            enabled = false;
            return;
        }

        var s = steps[i];

        // ���� ������ ������ �����ְ� ���� �� ���� ��⡱�� ���ϸ�:
        //   1) ���� �ѹ� �����ְ�
        //   2) Ŭ�� ���� �ٷ� �ٸ� �����
        //   3) ������ ��ٸ���
        StartCoroutine(RunStep(s));
    }

    IEnumerator RunStep(Step s)
    {
        // �ٸ� �����ָ� ���� ���
        if (s.hideBarWhileWaiting == false)
        {
            yield return StartCoroutine(FadeBar(1f));
            yield return ShowText(s.line, s.lineIcon);
        }
        else
        {
            // ���� ������ ������ �ڡ�
            yield return StartCoroutine(FadeBar(1f));
            yield return ShowText(s.line, s.lineIcon);
            // ���ٸ� ���� ä ���� ���
            yield return StartCoroutine(FadeBar(0f));
        }

        running = true;
    }

    IEnumerator ShowText(string line, Sprite sp)
    {
        if (icon) icon.sprite = sp;
        if (!message) yield break;

        if (!useTyping)
        {
            message.text = line;
            yield break;
        }

        typing = true;
        message.text = "";
        float t = 0f; int shown = 0; float cps = Mathf.Max(1f, charsPerSec);
        while (shown < line.Length)
        {
            t += Time.deltaTime * cps;
            int target = Mathf.Clamp(Mathf.FloorToInt(t), 0, line.Length);
            if (target != shown) { shown = target; message.text = line.Substring(0, shown); }
            // Ŭ�� �� ��� �ϼ�
            if (Input.GetMouseButtonDown(0)) { message.text = line; break; }
            yield return null;
        }
        message.text = line;
        typing = false;
    }

    IEnumerator FadeBar(float target)
    {
        if (!barGroup) yield break;
        float a0 = barGroup.alpha;
        barGroup.interactable = false; barGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.deltaTime;
            barGroup.alpha = Mathf.Lerp(a0, target, t / fadeSeconds);
            yield return null;
        }
        barGroup.alpha = target;

        // ���� ���� ��ȣ�ۿ� ���
        bool on = target > 0.99f;
        barGroup.interactable = on; barGroup.blocksRaycasts = on;
    }
}
