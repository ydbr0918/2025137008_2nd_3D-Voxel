using UnityEngine;
using TMPro;
using System.Collections;

public class NarrationUI : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup group;     // 바(패널) CanvasGroup
    public TMP_Text textField;    // 내레이션 TMP 텍스트

    [Header("Timing")]
    public float fade = 0.2f;     // 페이드 시간
    public float hold = 1.5f;     // 유지 시간(초)

    Coroutine co;

    void Awake()
    {
        if (group) { group.alpha = 0; group.blocksRaycasts = false; }
        if (textField) textField.text = "";
    }

    public void ShowOnce(string line, float? holdSeconds = null)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(ShowCo(line, holdSeconds ?? hold));
    }

    IEnumerator ShowCo(string line, float holdSec)
    {
        if (textField) textField.text = line;

        // fade in
        float t = 0; if (group) group.blocksRaycasts = false;
        while (t < fade) { t += Time.deltaTime; if (group) group.alpha = Mathf.Lerp(0, 1, t / fade); yield return null; }
        if (group) group.alpha = 1;

        // hold
        yield return new WaitForSeconds(holdSec);

        // fade out
        t = 0;
        while (t < fade) { t += Time.deltaTime; if (group) group.alpha = Mathf.Lerp(1, 0, t / fade); yield return null; }
        if (group) { group.alpha = 0; group.blocksRaycasts = false; }
        if (textField) textField.text = "";
        co = null;
    }
}
