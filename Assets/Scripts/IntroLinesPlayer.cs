using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IntroLinesPlayer : MonoBehaviour
{
    [Header("UI References (이미 씬에 있는 것 연결)")]
    public CanvasGroup barGroup;     // 텍스트 바(패널)에 CanvasGroup
    public TMP_Text message;       // 바 안의 TMP 텍스트
    public Image portrait;      // 바 왼쪽 아이콘(Image). 없으면 비워두기

    [Header("Lines & Icons")]
    [TextArea] public string[] lines;     // 차례대로 보여줄 대사들
    public Sprite defaultIcon;            // 공통 아이콘(선택)
    public Sprite[] lineIcons;            // 줄별 아이콘(선택, lines 길이에 맞추면 그 줄에만 적용)

    [Header("Text Settings")]
    public float fontSize = 36f;          // 텍스트 크기
    public bool wordWrap = true;

    [Header("Typing Effect")]
    public bool typeEffect = true;       // 타자 효과 사용 여부
    public float charsPerSec = 40f;       // 초당 글자 수

    [Header("Flow")]
    public bool autoStart = true;         // 시작하자마자 재생
    public bool clickToAdvance = true;    // 클릭/키로 다음 줄
    public KeyCode advanceKey = KeyCode.Mouse0; // 기본: 마우스 좌클릭
    public float fadeSeconds = 0.2f;      // 바 표시/숨김 페이드
    public UnityEvent onFinished;         // 끝나고 호출(다음 단계 트리거 등)

    int index = -1;            // 현재 줄 인덱스
    bool isTyping = false;     // 타이핑 중?
    string currentFullLine;    // 현재 줄 전체 텍스트
    Coroutine typingCo;

    void Start()
    {
        // 초기 세팅
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

    // 외부에서 호출해서 재생 시작하고 싶을 때
    public void StartPlayback()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        // 바 보이기
        yield return Fade(barGroup, barGroup ? barGroup.alpha : 0f, 1f, fadeSeconds);
        ShowNextLine();
    }

    // 클릭/키 입력 시 호출(버튼 OnClick에 연결해도 됨)
    public void Proceed()
    {
        if (isTyping)
        {
            // 타이핑 중이면 즉시 완성
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
            // 끝
            StartCoroutine(FinishRoutine());
            return;
        }

        // 아이콘 결정: 줄별 아이콘 > 기본 아이콘
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
        // 바 숨김
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
