using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPortal : MonoBehaviour
{
    public string nextSceneName;     // "B4" ∞∞¿∫ æ¿ ¿Ã∏ß

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        StartCoroutine(GoNext());
    }

    System.Collections.IEnumerator GoNext()
    {
        var cg = FindObjectOfType<CanvasGroup>();   // Overlay
        if (cg) yield return Fade(cg, 0f, 1f, 0.5f);
        yield return SceneManager.LoadSceneAsync(nextSceneName);
        if (cg) yield return Fade(cg, 1f, 0f, 0.5f);
    }

    System.Collections.IEnumerator Fade(CanvasGroup g, float a, float b, float d)
    {
        float t = 0f; while (t < d) { t += Time.unscaledDeltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
    }
}
