using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ElevatorInteraction : MonoBehaviour
{
    [Header("Power Check")]
    public PowerManager powerManager;       // 전원 카운트가 여기에 있음
    public ElevatorManager elevatorManager; // (백업) 불 3개 켜짐 검사

    [Header("Scene Names")]
    public string elevatorInteriorSceneName = "ElevatorInterior"; // 엘리베이터 내부 컷신 씬
    public string nextStageSceneName = "";                        // 다음 스테이지 씬 (권장)
    public int nextStageBuildIndex = -1;                          // 또는 빌드 인덱스

    [Header("Timing")]
    public float rideSeconds = 5f;     // 내부 보여줄 시간
    public float fadeSeconds = 0.35f;  // 컷신 씬에서 사용할 페이드 시간(옵션)

    bool isLoading;

    public void TryInteract()
    {
        if (isLoading) return;

        bool powered =
            (powerManager && powerManager.IsPowered) ||
            (!powerManager && elevatorManager && elevatorManager.AllOn);

        if (!powered) return; // 잠겨있으면 아무 것도 안 함(프롬프트/대사는 다른 곳에서 처리)

        StartCoroutine(GoInteriorThenAutoNext());
    }

    IEnumerator GoInteriorThenAutoNext()
    {
        isLoading = true;

        // 엘레베이터 프롬프트 F 잔상 입력 무시 (있다면)
        FindObjectOfType<ElevatorAimPromptAndTalk>()?.BlockInputForTeleport();

        // 컷신 씬에서 사용할 데이터 지정
        SceneTransitData.RideSeconds = rideSeconds;
        SceneTransitData.FadeSeconds = fadeSeconds;
        SceneTransitData.NextSceneName = nextStageSceneName;
        SceneTransitData.NextBuildIndex = nextStageBuildIndex;

        // 엘리베이터 내부 씬 로드
        if (!string.IsNullOrEmpty(elevatorInteriorSceneName))
        {
            SceneManager.LoadScene(elevatorInteriorSceneName);
        }
        else
        {
            Debug.LogError("[ElevatorInteraction] elevatorInteriorSceneName 이 비었습니다.");
        }
        yield break;
    }
}
