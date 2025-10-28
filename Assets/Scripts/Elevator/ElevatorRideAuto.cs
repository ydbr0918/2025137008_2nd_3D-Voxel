using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorRideAuto : MonoBehaviour
{
    [Header("Optional Fade")]
    public CanvasGroup fadeOverlay; // 알파 0으로 시작하는 검은 패널(화면 전체)
    public float fadeSecondsOverride = -1f; // 음수면 SceneTransitData.FadeSeconds 사용

    [Header("Optional Player/Camera Lock (컷신용)")]
    public MonoBehaviour[] playerControlScriptsToDisable; // PlayerController 등
    public Camera sceneCamera; // 비워두면 Camera.main 사용

    void Start()
    {
        StartCoroutine(RunCutscene());
    }

    IEnumerator RunCutscene()
    {
        float fadeSec = fadeSecondsOverride >= 0f ? fadeSecondsOverride : SceneTransitData.FadeSeconds;
        float rideSec = Mathf.Max(0f, SceneTransitData.RideSeconds);

        // 1) 페이드 인(검은 화면 → 내부 보이기)
        if (fadeOverlay) yield return FadeTo(fadeOverlay, 0f, fadeSec);

        // 2) 플레이어 조작 잠금 (있으면)
        foreach (var c in playerControlScriptsToDisable)
            if (c) c.enabled = false;

        // 3) 5초(설정값) 간 내부 보여주기
        float t = 0f;
        while (t < rideSec) { t += Time.deltaTime; yield return null; }

        // 4) 페이드 아웃(내부 → 검은 화면)
        if (fadeOverlay) yield return FadeTo(fadeOverlay, 1f, fadeSec);

        // 5) 다음 씬 로드
        if (!string.IsNullOrEmpty(SceneTransitData.NextSceneName))
        {
            SceneManager.LoadScene(SceneTransitData.NextSceneName);
        }
        else if (SceneTransitData.NextBuildIndex >= 0)
        {
            SceneManager.LoadScene(SceneTransitData.NextBuildIndex);
        }
        else
        {
            Debug.LogError("[ElevatorRideAuto] 다음 씬이 지정되지 않았습니다.");
        }
    }

    IEnumerator FadeTo(CanvasGroup g, float target, float seconds)
    {
        float a0 = g.alpha;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(a0, target, t / seconds);
            yield return null;
        }
        g.alpha = target;
    }
}
