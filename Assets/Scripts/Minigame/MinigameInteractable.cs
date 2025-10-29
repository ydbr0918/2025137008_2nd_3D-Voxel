// MinigameInteractable.cs
// 에임이 대상에 닿으면 [F] 프롬프트를 안정적으로 표시하고,
// F를 누르면 미니게임 패널을 열어 플레이어 조작을 잠그는 스크립트.
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class MinigameInteractable : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                          // 비우면 Camera.main
    [Range(0.5f, 10f)] public float maxDistance = 3.0f;
    public LayerMask hitMask = ~0;              // 권장: Interactable만 체크

    [Header("Aim Stability")]
    [Tooltip("중앙 레이 대신 구체 캐스트 반지름(조준 완화)")]
    public float aimRadius = 0.12f;
    [Tooltip("한 번 맞춘 뒤 이 시간(초) 동안은 프롬프트 유지(깜빡임 방지)")]
    public float holdGraceSeconds = 0.15f;

    [Header("Prompt UI")]
    public TMP_Text promptText;                 // [F] 열기 텍스트 (씬 오브젝트!)
    public string promptLine = "[F] 열기";

    [Header("Open Minigame")]
    public GameObject minigamePanel;            // 미니게임 패널(비활성 시작 권장)
    public KeyCode useKey = KeyCode.F;
    public bool lockPlayerWhileOpen = true;     // 열려있는 동안 플레이어 조작 잠금
    public MonoBehaviour[] playerControlScriptsToDisable; // PlayerController, 카메라 컨트롤 등

    [Header("Cursor Handling")]
    public bool showMouseCursorWhenOpen = true;

    [Header("Prompt Fix (Optional)")]
    [Tooltip("프롬프트가 보이지 않을 때 최상단/알파 보정")]
    public bool forceVisibleFix = true;
    public int bringToFrontSibling = 9999;

    // 내부 상태
    bool lookingAtMe = false;
    bool isOpen = false;
    bool readyForPress = true;
    float lastSeenTime = -999f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (minigamePanel) minigamePanel.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateAim();

        // 상호작용 키
        if (lookingAtMe && readyForPress && Input.GetKeyDown(useKey))
        {
            OpenPanel();
            readyForPress = false; // 같은 프레임 중복 방지
        }
        if (!Input.GetKey(useKey)) readyForPress = true;

        // ESC로 닫기(원하면)
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    void UpdateAim()
    {
        bool seenThisFrame = false;

        if (cam)
        {
            // 화면 중앙에서 스피어캐스트(조준 완화)
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.SphereCast(ray, aimRadius, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                var interactable = hit.collider.GetComponentInParent<MinigameInteractable>();
                if (interactable == this)
                {
                    seenThisFrame = true;
                    lastSeenTime = Time.time;
                }
            }
        }

        // 최근 본 지 잠깐 동안은 계속 보고 있는 것으로 처리(깜빡임 방지)
        bool hitMe = seenThisFrame || (Time.time - lastSeenTime <= holdGraceSeconds);
        lookingAtMe = hitMe;

        // ===== 프롬프트 표시/숨김 =====
        if (!promptText) return;

        if (hitMe && !isOpen)
        {
            if (!promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(true);

            if (promptText.text != promptLine)
                promptText.text = promptLine;

            if (forceVisibleFix)
            {
                // 알파/정렬/겹침 보정
                var c = promptText.color; c.a = 1f; promptText.color = c;

                var cg = promptText.GetComponentInParent<CanvasGroup>();
                if (cg) { cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false; }

                promptText.rectTransform.SetSiblingIndex(bringToFrontSibling);
            }
        }
        else
        {
            if (promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        if (isOpen) return;
        isOpen = true;

        if (promptText) promptText.gameObject.SetActive(false);
        if (minigamePanel) minigamePanel.SetActive(true);

        if (lockPlayerWhileOpen)
        {
            foreach (var s in playerControlScriptsToDisable)
                if (s) s.enabled = false;
        }

        if (showMouseCursorWhenOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ClosePanel()
    {
        if (!isOpen) return;
        isOpen = false;

        if (minigamePanel) minigamePanel.SetActive(false);

        if (lockPlayerWhileOpen)
        {
            foreach (var s in playerControlScriptsToDisable)
                if (s) s.enabled = true;
        }
        // 커서 복구 정책은 프로젝트 전반 룰에 맞춰 외부에서 관리해도 OK
    }

    // 미니게임 성공 이벤트에서 호출
    public void CloseAfterSuccess() => ClosePanel();
}
