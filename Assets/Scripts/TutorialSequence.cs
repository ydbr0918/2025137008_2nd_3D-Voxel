using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialSequence : MonoBehaviour
{
    [Header("UI refs")]
    public CanvasGroup barGroup;      // TutorialBottomBar의 CanvasGroup
    public Image icon;                // 선택
    public TMP_Text message;          // TMP 텍스트

    [Header("Typing")]
    public bool useTyping = true;
    public float charsPerSec = 40f;

    [Header("Fade")]
    public float fadeSeconds = 0.35f;

    [System.Serializable]
    public class Step
    {
        [TextArea] public string line;      // 스텝에서 표시할 문구
        public Sprite lineIcon;             // 선택 아이콘
        public Condition condition;         // 어떤 조건을 만족하면 스텝 완료?
        public bool hideBarWhileWaiting;    // 이 스텝에서는 바를 숨긴 채로 대기?
        public float distanceNeeded = 2.0f; // Move에서 누적 이동 거리 기준
        public int clicksNeeded = 3;        // Shoot 등 클릭 횟수 기준
        [TextArea] public string successLine; // 조건 달성 후 보여줄 “잘했어!” 문구 (선택)
        public bool waitForClickAfterSuccess; // 성공 문구 후 클릭 기다릴지(그 다음 스텝으로 즉시 넘어갈지)
    }

    public enum Condition
    {
        ClickToContinue,  // 클릭하면 다음
        MoveWASD,         // WASD 이동 누적
        SwitchViewV,      // V키 눌러 시점 변경
        AimRMB,           // 우클릭 조준 유지
        ShootLMB,         // 좌클릭 n회
        InteractE,        // E 상호작용 1회
        None              // 바로 다음(정보만)
    }

    [Header("Steps (순서대로)")]
    public Step[] steps;

    // 내부 상태
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
        // 시작 시 첫 스텝으로
        NextStep();
    }

    void Update()
    {
        if (!running) return;
        var s = steps[i];

        // ▼ 조건 업데이트
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
        // Player 위치 추적(씬에 Player 태그가 있다고 가정)
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

        // 성공 문구가 있고, 바를 보여주며 성공 피드백을 주고 싶다면
        if (!string.IsNullOrEmpty(s.successLine))
        {
            // 바가 숨겨져 있던 스텝이면 먼저 나타내기
            if (s.hideBarWhileWaiting) yield return StartCoroutine(FadeBar(1f));

            yield return ShowText(s.successLine, s.lineIcon);

            if (s.waitForClickAfterSuccess)
            {
                // 클릭을 기다렸다가 다음으로
                while (!Input.GetMouseButtonDown(0)) yield return null;
            }
        }

        // 다음 스텝으로
        NextStep();
    }

    void NextStep()
    {
        // 상태 리셋
        moved = 0f; aimHold = 0f; clicks = 0; interacted = false; viewSwitched = false; _lastValid = false;

        i++;
        if (i >= steps.Length)
        {
            // 전체 튜토리얼 종료: 바 숨김
            StartCoroutine(FadeBar(0f));
            enabled = false;
            return;
        }

        var s = steps[i];

        // 현재 스텝이 “문구 보여주고 나서 바 숨김 대기”를 원하면:
        //   1) 문구 한번 보여주고
        //   2) 클릭 없이 바로 바를 숨기고
        //   3) 조건을 기다린다
        StartCoroutine(RunStep(s));
    }

    IEnumerator RunStep(Step s)
    {
        // 바를 보여주며 문구 출력
        if (s.hideBarWhileWaiting == false)
        {
            yield return StartCoroutine(FadeBar(1f));
            yield return ShowText(s.line, s.lineIcon);
        }
        else
        {
            // 먼저 문구를 보여준 뒤…
            yield return StartCoroutine(FadeBar(1f));
            yield return ShowText(s.line, s.lineIcon);
            // …바를 숨긴 채 조건 대기
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
            // 클릭 시 즉시 완성
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

        // 보일 때만 상호작용 허용
        bool on = target > 0.99f;
        barGroup.interactable = on; barGroup.blocksRaycasts = on;
    }
}
