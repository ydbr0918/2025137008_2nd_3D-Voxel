using UnityEngine;
using TMPro;

public class ElevatorAimPromptAndTalk : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                    // 비워두면 Camera.main
    public float maxDistance = 3.0f;      // 상호작용 거리
    public string elevatorTag = "Elevator";
    public LayerMask hitMask = ~0;        // 맞출 레이어(기본: Everything)

    [Header("Prompt UI ([F] 열기)")]
    public TMP_Text promptText;           // 화면 중앙에 표시할 TMP
    public string promptLine = "[F] 열기";

    [Header("캐릭터 대사창 (기존 것 사용)")]
    public NarrationTextBarSafe characterBar; // ← 하나만 사용
    public Sprite characterIcon;              // 캐릭터 아이콘
    [TextArea] public string talkLine = "잠겨있는것같아";
    public float talkHoldSeconds = 1.5f;

    [Header("Key")]
    public KeyCode useKey = KeyCode.F;

    bool lookingAtElevator = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (hitMask == 0) hitMask = ~0; // 비어 있으면 Everything
    }

    void Update()
    {
        UpdateAim();

        if (lookingAtElevator && Input.GetKeyDown(useKey))
        {
            if (characterBar)
            {
                // ★ 대사창 GO가 꺼져 있으면 자동 활성
                if (!characterBar.gameObject.activeInHierarchy)
                    characterBar.gameObject.SetActive(true);
                // ★ 컴포넌트도 꺼져 있으면 켜기
                if (!characterBar.enabled) characterBar.enabled = true;

                characterBar.ShowCharacter(talkLine, characterIcon, talkHoldSeconds);
            }
            else
            {
                Debug.LogWarning("[ElevatorAimPromptAndTalk] 캐릭터 바가 연결되지 않았습니다.");
            }
        }
    }

    void UpdateAim()
    {
        bool hitElevator = false;

        if (cam)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                // 부모나 루트에 Elevator 태그가 있으면 OK
                var tr = hit.collider.transform;
                if (tr.CompareTag(elevatorTag) ||
                    (tr.parent && tr.parent.CompareTag(elevatorTag)) ||
                    tr.root.CompareTag(elevatorTag))
                {
                    hitElevator = true;
                }
            }
        }

        if (promptText)
        {
            promptText.gameObject.SetActive(hitElevator);
            if (hitElevator) promptText.text = promptLine;
        }

        lookingAtElevator = hitElevator;
    }

    void OnDisable()
    {
        if (promptText) promptText.gameObject.SetActive(false);
    }
}
