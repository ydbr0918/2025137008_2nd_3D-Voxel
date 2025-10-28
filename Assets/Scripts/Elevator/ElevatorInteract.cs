using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ElevatorInteract : MonoBehaviour
{
    public PowerManager powerManager;
    public NarrationUI narration;          // 내레이션 UI
    public string lockedLine = "잠겨있는 것 같다.";

    [Header("Keys / Prompt")]
    public KeyCode key = KeyCode.F;
    public bool showPrompt = true;
    public CanvasGroup promptGroup;        // [F] 상호작용 같은 작은 프롬프트(선택)
    public TMPro.TMP_Text promptText;      // 선택
    public string promptReady = "[F] 엘리베이터 타기";
    public string promptLocked = "[F] 확인";

    [Header("Door / 이동 처리")]
    public Animator doorAnimator;          // 문 열림 애니(선택)
    public string openTrigger = "Open";
    public MonoBehaviour sceneLoader;      // 씬 전환 스크립트(선택)
    public string loaderMethod = "LoadNext"; // public 메서드명

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
                // 열림
                if (doorAnimator) doorAnimator.SetTrigger(openTrigger);
                // 필요하면 씬 전환/엘리베이터 탑승 실행
                if (sceneLoader && !string.IsNullOrEmpty(loaderMethod))
                    sceneLoader.Invoke(loaderMethod, 0f);
            }
            else
            {
                // 잠김 내레이션
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
