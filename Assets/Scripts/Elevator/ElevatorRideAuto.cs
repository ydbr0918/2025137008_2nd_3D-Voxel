using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorRideAuto : MonoBehaviour
{
    [Header("Optional Fade")]
    public CanvasGroup fadeOverlay; // ���� 0���� �����ϴ� ���� �г�(ȭ�� ��ü)
    public float fadeSecondsOverride = -1f; // ������ SceneTransitData.FadeSeconds ���

    [Header("Optional Player/Camera Lock (�ƽſ�)")]
    public MonoBehaviour[] playerControlScriptsToDisable; // PlayerController ��
    public Camera sceneCamera; // ����θ� Camera.main ���

    void Start()
    {
        StartCoroutine(RunCutscene());
    }

    IEnumerator RunCutscene()
    {
        float fadeSec = fadeSecondsOverride >= 0f ? fadeSecondsOverride : SceneTransitData.FadeSeconds;
        float rideSec = Mathf.Max(0f, SceneTransitData.RideSeconds);

        // 1) ���̵� ��(���� ȭ�� �� ���� ���̱�)
        if (fadeOverlay) yield return FadeTo(fadeOverlay, 0f, fadeSec);

        // 2) �÷��̾� ���� ��� (������)
        foreach (var c in playerControlScriptsToDisable)
            if (c) c.enabled = false;

        // 3) 5��(������) �� ���� �����ֱ�
        float t = 0f;
        while (t < rideSec) { t += Time.deltaTime; yield return null; }

        // 4) ���̵� �ƿ�(���� �� ���� ȭ��)
        if (fadeOverlay) yield return FadeTo(fadeOverlay, 1f, fadeSec);

        // 5) ���� �� �ε�
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
            Debug.LogError("[ElevatorRideAuto] ���� ���� �������� �ʾҽ��ϴ�.");
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
