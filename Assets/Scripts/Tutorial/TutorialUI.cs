using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUI : MonoBehaviour
{
    [Header("Text Box (네가 만든 UI를 연결)")]
    public CanvasGroup textBox;          // 텍스트 상자 패널 (CanvasGroup 필수)
    public TMP_Text textField;           // TMP 텍스트 (문구)
    [Header("Typing")]
    public bool useTyping = true;
    public float charsPerSec = 40f;

    Coroutine _typingCo;

    public string autoFindTextBoxName = "tutorialTextBar";
    public string autoFindTextFieldName = "Text";


    void Awake()
    {
        if (textBox) { textBox.alpha = 0f; textBox.interactable = false; textBox.blocksRaycasts = false; }

        if (!textBox && !string.IsNullOrEmpty(autoFindTextBoxName))
        {
            var go = GameObject.Find(autoFindTextBoxName);
            if (go)
            {
                var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
                textBox = cg;
            }
        }
        if (!textField && !string.IsNullOrEmpty(autoFindTextFieldName))
        {
            var tf = GameObject.Find(autoFindTextFieldName);
            if (tf) textField = tf.GetComponent<TMPro.TMP_Text>();
        }
    }

    public IEnumerator ShowText(string line, float fadeSeconds = 0.3f)
    {
        if (!textBox || !textField) yield break;

        // 켜기
        yield return Fade(textBox, textBox.alpha, 1f, fadeSeconds, true);

        // 타이핑
        if (useTyping)
        {
            if (_typingCo != null) StopCoroutine(_typingCo);
            _typingCo = StartCoroutine(TypeCo(line));
            yield return _typingCo;
        }
        else
        {
            textField.text = line;
        }
    }

    public IEnumerator HideText(float fadeSeconds = 0.3f)
    {
        if (!textBox) yield break;
        yield return Fade(textBox, textBox.alpha, 0f, fadeSeconds, false);
    }

    public void SetInstant(string line)
    {
        if (!textField || !textBox) return;
        textField.text = line;
        textBox.alpha = 1f; textBox.interactable = true; textBox.blocksRaycasts = true;
    }

    IEnumerator TypeCo(string line)
    {
        textField.text = "";
        float t = 0f; int shown = 0; float cps = Mathf.Max(1f, charsPerSec);
        while (shown < line.Length)
        {
            t += Time.deltaTime * cps;
            int target = Mathf.Clamp(Mathf.FloorToInt(t), 0, line.Length);
            if (target != shown) { shown = target; textField.text = line.Substring(0, shown); }
            if (Input.GetMouseButtonDown(0)) { textField.text = line; break; } // 클릭 즉시 완성
            yield return null;
        }
        textField.text = line;
    }

    IEnumerator Fade(CanvasGroup g, float a, float b, float d, bool enableOnEnd)
    {
        g.interactable = false; g.blocksRaycasts = false;
        float t = 0f;
        while (t < d) { t += Time.deltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
        bool on = b > 0.99f;
        g.interactable = on; g.blocksRaycasts = on && enableOnEnd;
    }
}
