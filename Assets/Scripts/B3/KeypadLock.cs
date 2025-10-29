using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class KeypadLock : MonoBehaviour
{
    [Header("Code")]
    [Tooltip("정답 코드(예: 7381)")]
    [SerializeField] private string correctCode = "7381";
    [SerializeField] private int maxDigits = 4;
    [SerializeField] private int maxAttempts = 0;   // 0=무제한, 3=3회 제한

    [Header("UI")]
    [SerializeField] private TMP_Text inputSlotsText;   // 예: "_ _ _ _" 표시
    [SerializeField] private TMP_Text hintText;         // (선택) 힌트/상태 메시지
    [SerializeField] private string slotEmptySymbol = "_";
    [SerializeField] private string bulletFilled = "●"; // 입력된 자리 표시

    [Header("SFX (옵션)")]
    public AudioSource sfx;
    public AudioClip clickClip;
    public AudioClip backspaceClip;
    public AudioClip clearClip;
    public AudioClip successClip;
    public AudioClip failClip;

    [Header("Events")]
    public UnityEvent onSuccess;   // 정답 맞음
    public UnityEvent onFail;      // 제출했지만 틀림(회수 차감 포함)
    public UnityEvent onLockout;   // 시도 회수 다 씀

    private string current = "";
    private int attempts = 0;
    private bool lockedOut = false;

    void Start()
    {
        maxDigits = Mathf.Max(1, maxDigits);
        if (string.IsNullOrEmpty(correctCode)) correctCode = "0000";
        correctCode = correctCode.Trim();
        Redraw();
    }

    // ---------- 외부에서 버튼이 호출 ----------
    public void PressDigit(int d)
    {
        if (lockedOut) return;
        if (current.Length >= maxDigits) return;

        current += Mathf.Clamp(d, 0, 9).ToString();
        Play(clickClip);
        Redraw();
    }

    public void Backspace()
    {
        if (lockedOut) return;
        if (current.Length > 0)
        {
            current = current.Substring(0, current.Length - 1);
            Play(backspaceClip);
            Redraw();
        }
    }

    public void ClearAll()
    {
        if (lockedOut) return;
        current = "";
        Play(clearClip);
        Redraw();
    }

    public void Submit()
    {
        if (lockedOut) return;

        // 자리수 미달이면 그대로 실패 처리할지, 무시할지는 취향대로
        if (current.Length < maxDigits)
        {
            // 여기선 실패로 보지 않고 무시 + 힌트만
            SetHint($"자릿수 {maxDigits}자리 필요");
            return;
        }

        bool ok = string.Equals(current, correctCode);
        if (ok)
        {
            Play(successClip);
            SetHint("해제 성공");
            onSuccess?.Invoke();
        }
        else
        {
            Play(failClip);
            attempts++;
            onFail?.Invoke();

            if (maxAttempts > 0 && attempts >= maxAttempts)
            {
                lockedOut = true;
                SetHint("잠김(시도 횟수 초과)");
                onLockout?.Invoke();
            }
            else
            {
                SetHint("암호가 틀렸습니다");
            }
        }

        // 성공/실패 후에는 입력 비우기(원하면 유지 가능)
        current = "";
        Redraw();
    }

    // ---------- 내부 ----------
    void Redraw()
    {
        if (inputSlotsText)
        {
            // 예시: "● ● _ _" 형식
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < maxDigits; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(i < current.Length ? bulletFilled : slotEmptySymbol);
            }
            inputSlotsText.text = sb.ToString();
        }
    }

    void SetHint(string msg)
    {
        if (hintText) hintText.text = msg;
    }

    void Play(AudioClip clip)
    {
        if (sfx && clip) sfx.PlayOneShot(clip);
    }

    // 필요하면 외부에서 정답/제한 동적으로 바꾸기
    public void SetCorrectCode(string code) => correctCode = code ?? "";
    public string GetCurrent() => current;
}
