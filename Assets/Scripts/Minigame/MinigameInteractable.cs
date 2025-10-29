using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class MinigameInteractable : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                      // ����θ� Camera.main
    public float maxDistance = 3.0f;        // ��ȣ�ۿ� �Ÿ�
    public LayerMask hitMask = ~0;          // ���� ���̾�(�⺻ Everything)

    [Header("Prompt UI")]
    public TMP_Text promptText;             // ȭ�� �߾� [F] ���� �ؽ�Ʈ
    public string promptLine = "[F] ����";

    [Header("Open Minigame")]
    public GameObject minigamePanel;        // TimingLockMinigame�� ����ִ� �г�(��Ȱ�� ���� ����)
    public KeyCode useKey = KeyCode.F;
    public bool lockPlayerWhileOpen = true; // �����ִ� ���� �÷��̾� ���� ���
    public MonoBehaviour[] playerControlScriptsToDisable; // PlayerController, MouseLook ��

    [Header("Cursor Handling")]
    public bool showMouseCursorWhenOpen = true;

    // ���� ����
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
            readyForPress = false; // ���� ������ �ߺ� ����
        }

        if (!Input.GetKey(useKey)) readyForPress = true;

        // ESC�� �ݱ�(����)
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
                // ���� �¾Ұų� �� �ڽ�/�θ� �¾Ҵ���
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

        // �÷��̾� ���� ��ױ�
        if (lockPlayerWhileOpen)
        {
            foreach (var s in playerControlScriptsToDisable)
                if (s) s.enabled = false;
        }

        // Ŀ�� ���̱�
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

        // �÷��̾� ���� �ǵ�����
        if (lockPlayerWhileOpen)
        {
            foreach (var s in playerControlScriptsToDisable)
                if (s) s.enabled = true;
        }

        // Ŀ�� ����(���ϸ� ������Ʈ ��å�� �°� ����)
        // ���⼭�� ���� ����� �ʰ�, �ܺ� ī�޶� ��ũ��Ʈ�� �����ϵ��� ��.
    }

    // TimingLockMinigame�� OnUnlocked �̺�Ʈ���� ȣ���ϸ� ���� ����
    public void CloseAfterSuccess()
    {
        ClosePanel();
    }
}
