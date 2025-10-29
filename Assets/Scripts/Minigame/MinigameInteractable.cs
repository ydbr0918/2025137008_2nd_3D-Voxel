// MinigameInteractable.cs
// ������ ��� ������ [F] ������Ʈ�� ���������� ǥ���ϰ�,
// F�� ������ �̴ϰ��� �г��� ���� �÷��̾� ������ ��״� ��ũ��Ʈ.
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class MinigameInteractable : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                          // ���� Camera.main
    [Range(0.5f, 10f)] public float maxDistance = 3.0f;
    public LayerMask hitMask = ~0;              // ����: Interactable�� üũ

    [Header("Aim Stability")]
    [Tooltip("�߾� ���� ��� ��ü ĳ��Ʈ ������(���� ��ȭ)")]
    public float aimRadius = 0.12f;
    [Tooltip("�� �� ���� �� �� �ð�(��) ������ ������Ʈ ����(������ ����)")]
    public float holdGraceSeconds = 0.15f;

    [Header("Prompt UI")]
    public TMP_Text promptText;                 // [F] ���� �ؽ�Ʈ (�� ������Ʈ!)
    public string promptLine = "[F] ����";

    [Header("Open Minigame")]
    public GameObject minigamePanel;            // �̴ϰ��� �г�(��Ȱ�� ���� ����)
    public KeyCode useKey = KeyCode.F;
    public bool lockPlayerWhileOpen = true;     // �����ִ� ���� �÷��̾� ���� ���
    public MonoBehaviour[] playerControlScriptsToDisable; // PlayerController, ī�޶� ��Ʈ�� ��

    [Header("Cursor Handling")]
    public bool showMouseCursorWhenOpen = true;

    [Header("Prompt Fix (Optional)")]
    [Tooltip("������Ʈ�� ������ ���� �� �ֻ��/���� ����")]
    public bool forceVisibleFix = true;
    public int bringToFrontSibling = 9999;

    // ���� ����
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

        // ��ȣ�ۿ� Ű
        if (lookingAtMe && readyForPress && Input.GetKeyDown(useKey))
        {
            OpenPanel();
            readyForPress = false; // ���� ������ �ߺ� ����
        }
        if (!Input.GetKey(useKey)) readyForPress = true;

        // ESC�� �ݱ�(���ϸ�)
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    void UpdateAim()
    {
        bool seenThisFrame = false;

        if (cam)
        {
            // ȭ�� �߾ӿ��� ���Ǿ�ĳ��Ʈ(���� ��ȭ)
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

        // �ֱ� �� �� ��� ������ ��� ���� �ִ� ������ ó��(������ ����)
        bool hitMe = seenThisFrame || (Time.time - lastSeenTime <= holdGraceSeconds);
        lookingAtMe = hitMe;

        // ===== ������Ʈ ǥ��/���� =====
        if (!promptText) return;

        if (hitMe && !isOpen)
        {
            if (!promptText.gameObject.activeSelf)
                promptText.gameObject.SetActive(true);

            if (promptText.text != promptLine)
                promptText.text = promptLine;

            if (forceVisibleFix)
            {
                // ����/����/��ħ ����
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
        // Ŀ�� ���� ��å�� ������Ʈ ���� �꿡 ���� �ܺο��� �����ص� OK
    }

    // �̴ϰ��� ���� �̺�Ʈ���� ȣ��
    public void CloseAfterSuccess() => ClosePanel();
}
