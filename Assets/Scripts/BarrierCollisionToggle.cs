using UnityEngine;



public class BarrierCollisionToggle : MonoBehaviour
{
    [Header("Refs")]
    public CameraSwitch camSwitch;          // 네가 쓰는 카메라 스위치

    [Header("Layer Names")]
    public string playerLayerName = "Player";
    public string barrierLayerName = "Barrier";   // 투명벽 레이어

    int playerLayer = -1;
    int barrierLayer = -1;
    bool lastIsFirstPerson;

    void Awake()
    {
        if (!camSwitch && Camera.main) camSwitch = Camera.main.GetComponent<CameraSwitch>();

        playerLayer = LayerMask.NameToLayer(playerLayerName);
        barrierLayer = LayerMask.NameToLayer(barrierLayerName);

        if (playerLayer < 0 || barrierLayer < 0)
        {
            Debug.LogError("[BarrierCollisionToggle] Layer 이름을 확인하세요. (Player, Barrier)");
            enabled = false;
            return;
        }

        // 시작 상태 반영
        lastIsFirstPerson = camSwitch ? camSwitch.IsFirstPerson : false;
        ApplyState(lastIsFirstPerson);
    }

    void Update()
    {
        if (!camSwitch) return;
        if (camSwitch.IsFirstPerson != lastIsFirstPerson)
        {
            lastIsFirstPerson = camSwitch.IsFirstPerson;
            ApplyState(lastIsFirstPerson);
        }
    }

    void ApplyState(bool isFirstPerson)
    {
        // 3인칭(= isFirstPerson == false)일 때 Barrier 충돌 무시
        bool ignoreBarrier = !isFirstPerson;

        Physics.IgnoreLayerCollision(playerLayer, barrierLayer, ignoreBarrier);

        // 참고: 양방향 설정이므로 한 번 호출로 충분
        // 일반 벽(Wall)은 손대지 않음 → 항상 충돌됨 (레이어 매트릭스 기본값)
#if UNITY_EDITOR
        Debug.Log($"[BarrierCollisionToggle] IsFP={isFirstPerson} → Ignore(Player,Barrier)={ignoreBarrier}");
#endif
    }
}
