using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorInteraction : MonoBehaviour
{
    [Header("Power Check")]
    public PowerManager powerManager;       // 전원 카운트
    public ElevatorManager elevatorManager; // (백업) 불 3개 켜짐 검사

    [Header("Locked UI (잠금 상태일 때 사용)")]
    public NarrationTextBarSafe textBar;
    public Sprite characterIcon;
    public string lockedText = "잠겨있는것같아";
    public float holdSeconds = 1.5f;

    [Header("Scene Names")]
    public string elevatorInteriorSceneName = "InElevator"; // 엘리베이터 내부 컷신 씬
    public string nextStageSceneName = "";                   // 다음 스테이지 씬 (권장)
    public int nextStageBuildIndex = -1;                     // 또는 빌드 인덱스

    [Header("Timing")]
    public float rideSeconds = 5f;     // 내부 보여줄 시간
    public float fadeSeconds = 0.35f;  // 내부 씬에서 사용할 페이드 시간

    [Header("Optional")]
    public float extraDistanceCheck = 0f;

    bool isLoading;

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
            // 🔒 잠금 상태 → 대사 출력 유지
            textBar?.ShowCharacter(lockedText, characterIcon, holdSeconds);
            return;
        }

        // ✅ 전원 완료 → InElevator(내부) 씬으로 진입
        StartCoroutine(GoInteriorThenAutoNext());
    }

    IEnumerator GoInteriorThenAutoNext()
    {
        isLoading = true;

        // 프롬프트 잔상 입력 무시(있다면)
        FindObjectOfType<ElevatorAimPromptAndTalk>()?.BlockInputForTeleport();

        // 내부 씬에서 사용할 전역 데이터 설정
        SceneTransitData.RideSeconds = rideSeconds;
        SceneTransitData.FadeSeconds = fadeSeconds;
        SceneTransitData.NextSceneName = nextStageSceneName;
        SceneTransitData.NextBuildIndex = nextStageBuildIndex;

        if (!string.IsNullOrEmpty(elevatorInteriorSceneName))
        {
            SceneManager.LoadScene(elevatorInteriorSceneName);
        }
        else
        {
            Debug.LogError("[ElevatorInteraction] elevatorInteriorSceneName 이 비어있습니다.");
        }

        yield break;
    }
}
