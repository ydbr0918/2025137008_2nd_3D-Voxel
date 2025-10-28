using UnityEngine;
using TMPro;

public class ElevatorAimPromptAndTalk : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                    // 비워두면 Camera.main
    public float maxDistance = 3.0f;      // 상호작용 거리
    public LayerMask hitMask = ~0;        // 맞출 레이어(기본: Everything)

    [Header("Prompt UI ([F] 열기)")]
    public TMP_Text promptText;
    public string promptLine = "[F] 열기";

    [Header("캐릭터 대사창")]
    public NarrationTextBarSafe characterBar;
    public Sprite characterIcon;
    [TextArea] public string talkLine = "잠겨있는것같아";
    public float talkHoldSeconds = 1.5f;

    [Header("Key / Debounce")]
    public KeyCode useKey = KeyCode.F;
    [Tooltip("텔레포트/문 상호작용 직후 F 입력 무시 시간(초)")]
    public float ignoreAfterTeleport = 0.2f;

    // 내부 상태
    bool lookingAtElevator = false;
    bool readyForPress = true;         // F를 한번 눌렀다면, 키를 뗄 때까지 대기
    float blockInputUntil = 0f;        // 이 시간 전까지는 F 무시

    // ──────────────────────────────────────────────────────────────
    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (hitMask == 0) hitMask = ~0;
    }

    void Update()
    {
        UpdateAim();

        // 프롬프트가 보이고, 키가 허용 상태이며, 이번 프레임에 눌렸을 때만
        if (lookingAtElevator
            && Time.time >= blockInputUntil
            && readyForPress
            && Input.GetKeyDown(useKey))
        {
            // 대사창 열기
            if (characterBar)
            {
                if (!characterBar.gameObject.activeInHierarchy)
                    characterBar.gameObject.SetActive(true);
                if (!characterBar.enabled) characterBar.enabled = true;

                characterBar.ShowCharacter(talkLine, characterIcon, talkHoldSeconds);
            }
            else
            {
                Debug.LogWarning("[ElevatorAimPromptAndTalk] characterBar가 비어있습니다.");
            }

            // 누른 뒤에는 키를 뗄 때까지 대기(같은 프레임 중복 방지)
            readyForPress = false;
        }

        // 키 해제 감지 → 다시 누를 준비 완료
        if (!Input.GetKey(useKey))
            readyForPress = true;
    }

    void UpdateAim()
    {
        bool hitElevator = false;

        if (cam)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                // ★ElevatorInteractable 컴포넌트가 붙은 것만 상호작용 허용
                var comp = hit.collider.GetComponentInParent<ElevatorInteractable>();
                if (comp != null && comp.isActiveAndEnabled)
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

    // ── 외부(문/텔레포트 스크립트)에서 호출: 텔레포트 직후 F 무시 ──
    public void BlockInputForTeleport()
    {
        blockInputUntil = Time.time + ignoreAfterTeleport;
        readyForPress = false; // 반드시 키를 한 번 떼고 다시 누르게
    }
}
