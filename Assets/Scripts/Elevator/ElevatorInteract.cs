using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ElevatorInteract : MonoBehaviour
{
    public PowerManager powerManager;
    public NarrationUI narration;          // �����̼� UI
    public string lockedLine = "����ִ� �� ����.";

    [Header("Keys / Prompt")]
    public KeyCode key = KeyCode.F;
    public bool showPrompt = true;
    public CanvasGroup promptGroup;        // [F] ��ȣ�ۿ� ���� ���� ������Ʈ(����)
    public TMPro.TMP_Text promptText;      // ����
    public string promptReady = "[F] ���������� Ÿ��";
    public string promptLocked = "[F] Ȯ��";

    [Header("Door / �̵� ó��")]
    public Animator doorAnimator;          // �� ���� �ִ�(����)
    public string openTrigger = "Open";
    public MonoBehaviour sceneLoader;      // �� ��ȯ ��ũ��Ʈ(����)
    public string loaderMethod = "LoadNext"; // public �޼����

    bool inRange = false;

    void Start()
    {
        if (promptGroup) { promptGroup.alpha = 0; promptGroup.blocksRaycasts = false; }
        UpdatePrompt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = true;
        UpdatePrompt();
    }
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = false;
        UpdatePrompt();
    }

    void Update()
    {
        if (!inRange) return;
        if (Input.GetKeyDown(key))
        {
            if (powerManager && powerManager.IsPowered)
            {
                // ����
                if (doorAnimator) doorAnimator.SetTrigger(openTrigger);
                // �ʿ��ϸ� �� ��ȯ/���������� ž�� ����
                if (sceneLoader && !string.IsNullOrEmpty(loaderMethod))
                    sceneLoader.Invoke(loaderMethod, 0f);
            }
            else
            {
                // ��� �����̼�
                narration?.ShowOnce(lockedLine, 1.5f);
            }
        }
    }

    void UpdatePrompt()
    {
        if (!showPrompt || !promptGroup) return;

        bool canUse = powerManager && powerManager.IsPowered;
        promptGroup.alpha = inRange ? 1f : 0f;
        if (promptText) promptText.text = inRange ? (canUse ? promptReady : promptLocked) : "";
    }
}
