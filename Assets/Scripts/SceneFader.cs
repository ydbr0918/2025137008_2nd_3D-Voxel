using UnityEngine;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Overlay (Black Fullscreen)")]
    public CanvasGroup overlay;   
    public float fadeDuration = 1.0f;
    public float holdBlackSeconds = 0.2f; 

    [Header("Tutorial Bar")]
    public CanvasGroup tutorialBar; 
    public float barFadeDuration = 0.35f;

    void Start()
    {
        StartCoroutine(FadeInSequence());
    }

    IEnumerator FadeInSequence()
    {
        if (overlay)
        {
            overlay.alpha = 1f;
            overlay.interactable = true;
            overlay.blocksRaycasts = true;
        }
        yield return new WaitForSeconds(holdBlackSeconds);

      
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (overlay) overlay.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        if (overlay) { overlay.alpha = 0f; overlay.interactable = false; overlay.blocksRaycasts = false; }

    
        if (tutorialBar)
        {
            tutorialBar.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(tutorialBar, 0f, 1f, barFadeDuration));
        }

    }

    IEnumerator FadeCanvasGroup(CanvasGroup g, float a, float b, float d)
    {
        g.alpha = a;
        g.interactable = false; g.blocksRaycasts = false;
        float t = 0f;
        while (t < d) { t += Time.deltaTime; g.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        g.alpha = b;
     
        g.interactable = true; g.blocksRaycasts = true;
    }
}
