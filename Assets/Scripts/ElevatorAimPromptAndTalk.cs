using UnityEngine;
using TMPro;

public class ElevatorAimPromptAndTalk : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                    // ����θ� Camera.main
    public float maxDistance = 3.0f;      // ��ȣ�ۿ� �Ÿ�
    public LayerMask hitMask = ~0;        // ���� ���̾�(�⺻: Everything)

    [Header("Prompt UI ([F] ����)")]
    public TMP_Text promptText;
    public string promptLine = "[F] ����";

    [Header("ĳ���� ���â")]
    public NarrationTextBarSafe characterBar;
    public Sprite characterIcon;
    [TextArea] public string talkLine = "����ִ°Ͱ���";
    public float talkHoldSeconds = 1.5f;

    [Header("Key / Debounce")]
    public KeyCode useKey = KeyCode.F;
    [Tooltip("�ڷ���Ʈ/�� ��ȣ�ۿ� ���� F �Է� ���� �ð�(��)")]
    public float ignoreAfterTeleport = 0.2f;

    // ���� ����
    bool lookingAtElevator = false;
    bool readyForPress = true;         // F�� �ѹ� �����ٸ�, Ű�� �� ������ ���
    float blockInputUntil = 0f;        // �� �ð� �������� F ����

    // ����������������������������������������������������������������������������������������������������������������������������
    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (hitMask == 0) hitMask = ~0;
    }

    void Update()
    {
        UpdateAim();

        // ������Ʈ�� ���̰�, Ű�� ��� �����̸�, �̹� �����ӿ� ������ ����
        if (lookingAtElevator
            && Time.time >= blockInputUntil
            && readyForPress
            && Input.GetKeyDown(useKey))
        {
            // ���â ����
            if (characterBar)
            {
                if (!characterBar.gameObject.activeInHierarchy)
                    characterBar.gameObject.SetActive(true);
                if (!characterBar.enabled) characterBar.enabled = true;

                characterBar.ShowCharacter(talkLine, characterIcon, talkHoldSeconds);
            }
            else
            {
                Debug.LogWarning("[ElevatorAimPromptAndTalk] characterBar�� ����ֽ��ϴ�.");
            }

            // ���� �ڿ��� Ű�� �� ������ ���(���� ������ �ߺ� ����)
            readyForPress = false;
        }

        // Ű ���� ���� �� �ٽ� ���� �غ� �Ϸ�
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
                // ��ElevatorInteractable ������Ʈ�� ���� �͸� ��ȣ�ۿ� ���
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

    // ���� �ܺ�(��/�ڷ���Ʈ ��ũ��Ʈ)���� ȣ��: �ڷ���Ʈ ���� F ���� ����
    public void BlockInputForTeleport()
    {
        blockInputUntil = Time.time + ignoreAfterTeleport;
        readyForPress = false; // �ݵ�� Ű�� �� �� ���� �ٽ� ������
    }
}
