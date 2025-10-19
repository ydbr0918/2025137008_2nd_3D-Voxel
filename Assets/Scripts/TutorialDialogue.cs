using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class TutorialDialogue : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup barGroup;      // TutorialBottomBar의 CanvasGroup
    public Image icon;                // 선택
    public TMP_Text message;          // TMP 텍스트

    [Header("Texts")]
    [TextArea] public string firstLine = "튜토리얼에 온걸 환영해.";
    [TextArea] public string secondLine = "게임을 플레이하기전에 간단하게 조작법을 알아보자.";

    [Header("Typing")]
    public bool typeEffect = true;
    public float charsPerSec = 40f;

    [Header("Fade Out On Finish")]
    public float fadeOutSeconds = 0.35f;

    // 외부에서 구독 가능(대사창이 완전히 사라진 뒤 호출)
    public event Action OnDialogueFinished;

    // 0: 첫 문구, 1: 두 번째 문구, 2: 종료 대기, 3: 끝
    int state = 0;
    bool inputLocked = false; // 더블클릭로 스킵되는 것 방지

    void OnEnable()
    {
        // 첫 줄 표시
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

    public void Proceed() // 버튼 OnClick으로도 호출 가능
    {
        if (state >= 3 || inputLocked) return;

        if (state == 0)
        {
            // 첫 문구 도중 클릭: 즉시 완성, 이미 완성이면 다음 줄
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
        inputLocked = true; // 연속 클릭 방지

        if (barGroup)
        {
            barGroup.interactable = false; // 클릭 막기
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

        // 바 자체 비활성화(원하면 Destroy로 바꿔도 됨)
        gameObject.SetActive(false);

        state = 3;
        inputLocked = false;
        OnDialogueFinished?.Invoke(); // 다음 시스템(튜토리얼 단계) 시작 신호
    }
}
