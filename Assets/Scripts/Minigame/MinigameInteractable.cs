using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class MinigameInteractable : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                      // 비워두면 Camera.main
    public float maxDistance = 3.0f;        // 상호작용 거리
    public LayerMask hitMask = ~0;          // 맞출 레이어(기본 Everything)

    [Header("Prompt UI")]
    public TMP_Text promptText;             // 화면 중앙 [F] 열기 텍스트
    public string promptLine = "[F] 열기";

    [Header("Open Minigame")]
    public GameObject minigamePanel;        // TimingLockMinigame가 들어있는 패널(비활성 시작 권장)
    public KeyCode useKey = KeyCode.F;
    public bool lockPlayerWhileOpen = true; // 열려있는 동안 플레이어 조작 잠금
    public MonoBehaviour[] playerControlScriptsToDisable; // PlayerController, MouseLook 등

    [Header("Cursor Handling")]
    public bool showMouseCursorWhenOpen = true;

    // 내부 상태
    bool lookingAtMe = false;
    bool isOpen = false;
    bool readyForPress = true;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (minigamePanel) minigamePanel.SetActive(false);
    }

    void Update()
    {
        UpdateAim();

        if (lookingAtMe && readyForPress && Input.GetKeyDown(useKey))
        {
            OpenPanel();
            readyForPress = false; // 같은 프레임 중복 방지
        }

        if (!Input.GetKey(useKey)) readyForPress = true;

        // ESC로 닫기(선택)
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    void UpdateAim()
    {
        bool hitMe = false;

        if (cam)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                // 내가 맞았거나 내 자식/부모를 맞았는지
                var mine = GetComponent<Collider>();
                if (hit.collider && (hit.collider == mine ||
                    hit.collider.GetComponentInParent<MinigameInteractable>() == this))
                {
                    hitMe = true;
                }
            }
        }

        if (promptText)
        {
            promptText.gameObject.SetActive(hitMe && !isOpen);
            if (hitMe) promptText.text = promptLine;
        }

        lookingAtMe = hitMe;
    }

    public void OpenPanel()
    {
        if (isOpen) return;
        isOpen = true;

        if (promptText) promptText.gameObject.SetActive(false);
        if (minigamePanel) minigamePanel.SetActive(true);

        // 플레이어 조작 잠그기
        if (lockPlayerWhileOpen)
        {
            foreach (var s in playerControlScriptsToDisable)
                if (s) s.enabled = false;
        }

        // 커서 보이기
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

        // 플레이어 조작 되돌리기
        if (lockPlayerWhileOpen)
        {
            foreach (var s in playerControlScriptsToDisable)
                if (s) s.enabled = true;
        }

        // 커서 원상(원하면 프로젝트 정책에 맞게 조정)
        // 여기서는 굳이 잠그지 않고, 외부 카메라 스크립트가 관리하도록 둠.
    }

    // TimingLockMinigame의 OnUnlocked 이벤트에서 호출하면 편한 헬퍼
    public void CloseAfterSuccess()
    {
        ClosePanel();
    }
}
