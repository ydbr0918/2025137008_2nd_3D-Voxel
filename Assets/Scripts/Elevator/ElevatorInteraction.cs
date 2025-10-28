using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorInteraction : MonoBehaviour
{
    [Header("Refs")]
    public PowerManager powerManager;             // 있으면 우선 사용
    public ElevatorManager elevatorManager;       // 없거나 백업 용도
    public NarrationTextBarSafe textBar;          // 잠김일 때만 사용(대사 출력)

    [Header("Locked UI")]
    public Sprite characterIcon;
    public string lockedText = "잠겨있는것같아";
    public float holdSeconds = 1.5f;

    [Header("Load Next Scene")]
    public string nextSceneName = "";             // 씬 이름 (권장)
    public int nextBuildIndex = -1;               // 또는 빌드 인덱스(둘 중 하나만 쓰기)
    public bool useAsyncLoad = true;              // 비동기 로드(끊김 줄이기)

    [Header("Optional Fade Out")]
    public CanvasGroup fadeOverlay;               // 화면 덮는 검은 CanvasGroup (0~1)
    public float fadeSeconds = 0.35f;             // 0이면 페이드 없이 바로 로드

    [Header("Optional")]
    public float extraDistanceCheck = 0f;         // >0이면 상호작용 시 거리 검증

    bool isLoading = false;

    public void TryInteract()
    {
        if (isLoading) return;

        if (extraDistanceCheck > 0f && Camera.main)
        {
            float d = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (d > extraDistanceCheck) return;
        }

        bool powered =
            (powerManager && powerManager.IsPowered) ||
            (!powerManager && elevatorManager && elevatorManager.AllOn);

        if (!powered)
        {
            // 🔒 아직 잠김 → 대사만 출력
            textBar?.ShowCharacter(lockedText, characterIcon, holdSeconds);
            return;
        }

        // ✅ 전원 완료 → 씬 이동
        StartCoroutine(GoNextScene());
    }

    IEnumerator GoNextScene()
    {
        isLoading = true;

        // F 잔상 입력 방지(있으면)
        var prompt = FindObjectOfType<ElevatorAimPromptAndTalk>();
        prompt?.BlockInputForTeleport();

        // 페이드 아웃
        if (fadeOverlay && fadeSeconds > 0f)
        {
            float t = 0f, a0 = fadeOverlay.alpha;
            while (t < fadeSeconds)
            {
                t += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Lerp(a0, 1f, t / fadeSeconds);
                yield return null;
            }
            fadeOverlay.alpha = 1f;
        }

        // 씬 로드
        if (useAsyncLoad)
        {
            AsyncOperation op;
            if (!string.IsNullOrEmpty(nextSceneName))
                op = SceneManager.LoadSceneAsync(nextSceneName);
            else if (nextBuildIndex >= 0)
                op = SceneManager.LoadSceneAsync(nextBuildIndex);
            else
            {
                Debug.LogError("[ElevatorInteraction] 다음 씬이 지정되지 않았습니다.");
                yield break;
            }

            op.allowSceneActivation = true; // 페이드 완료했으니 바로 진입
            while (!op.isDone) yield return null;
        }
        else
        {
            if (!string.IsNullOrEmpty(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else if (nextBuildIndex >= 0)
                SceneManager.LoadScene(nextBuildIndex);
            else
                Debug.LogError("[ElevatorInteraction] 다음 씬이 지정되지 않았습니다.");
        }
    }
}
