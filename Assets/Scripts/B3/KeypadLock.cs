using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class KeypadLock : MonoBehaviour
{
    [Header("Code")]
    [Tooltip("���� �ڵ�(��: 7381)")]
    [SerializeField] private string correctCode = "7381";
    [SerializeField] private int maxDigits = 4;
    [SerializeField] private int maxAttempts = 0;   // 0=������, 3=3ȸ ����

    [Header("UI")]
    [SerializeField] private TMP_Text inputSlotsText;   // ��: "_ _ _ _" ǥ��
    [SerializeField] private TMP_Text hintText;         // (����) ��Ʈ/���� �޽���
    [SerializeField] private string slotEmptySymbol = "_";
    [SerializeField] private string bulletFilled = "��"; // �Էµ� �ڸ� ǥ��

    [Header("SFX (�ɼ�)")]
    public AudioSource sfx;
    public AudioClip clickClip;
    public AudioClip backspaceClip;
    public AudioClip clearClip;
    public AudioClip successClip;
    public AudioClip failClip;

    [Header("Events")]
    public UnityEvent onSuccess;   // ���� ����
    public UnityEvent onFail;      // ���������� Ʋ��(ȸ�� ���� ����)
    public UnityEvent onLockout;   // �õ� ȸ�� �� ��

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

    // ---------- �ܺο��� ��ư�� ȣ�� ----------
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

        // �ڸ��� �̴��̸� �״�� ���� ó������, ���������� ������
        if (current.Length < maxDigits)
        {
            // ���⼱ ���з� ���� �ʰ� ���� + ��Ʈ��
            SetHint($"�ڸ��� {maxDigits}�ڸ� �ʿ�");
            return;
        }

        bool ok = string.Equals(current, correctCode);
        if (ok)
        {
            Play(successClip);
            SetHint("���� ����");
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
                SetHint("���(�õ� Ƚ�� �ʰ�)");
                onLockout?.Invoke();
            }
            else
            {
                SetHint("��ȣ�� Ʋ�Ƚ��ϴ�");
            }
        }

        // ����/���� �Ŀ��� �Է� ����(���ϸ� ���� ����)
        current = "";
        Redraw();
    }

    // ---------- ���� ----------
    void Redraw()
    {
        if (inputSlotsText)
        {
            // ����: "�� �� _ _" ����
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

    // �ʿ��ϸ� �ܺο��� ����/���� �������� �ٲٱ�
    public void SetCorrectCode(string code) => correctCode = code ?? "";
    public string GetCurrent() => current;
}
