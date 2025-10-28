using UnityEngine;

public class ElevatorInteraction : MonoBehaviour
{
    [Header("Refs")]
    public PowerManager powerManager;             // 있으면 우선 사용
    public ElevatorManager elevatorManager;       // 없거나 백업 용도
    public NarrationTextBarSafe textBar;

    [Header("UI Texts")]
    public Sprite characterIcon;
    public string lockedText = "잠겨있는것같아";
    public string unlockedText = "엘리베이터를 탑승할 수 있다.";
    public float holdSeconds = 1.5f;

    [Header("Optional")]
    public float extraDistanceCheck = 0f; // >0이면 상호작용 시 거리 검증

    public void TryInteract()
    {
        if (extraDistanceCheck > 0f && Camera.main)
        {
            float d = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (d > extraDistanceCheck) return;
        }

        bool powered =
            (powerManager && powerManager.IsPowered) ||
            (!powerManager && elevatorManager && elevatorManager.AllOn);

        if (powered)
        {
            textBar?.ShowCharacter(unlockedText, characterIcon, holdSeconds);
            Debug.Log("[ElevatorInteraction] POWERED: 탑승 로직 실행");
            // TODO: 여기서 씬 전환/엘리베이터 이동 실행
        }
        else
        {
            textBar?.ShowCharacter(lockedText, characterIcon, holdSeconds);
        }
    }
}
