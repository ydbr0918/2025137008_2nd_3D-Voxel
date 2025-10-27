using UnityEngine;
using TMPro;

public class ElevatorAimPromptAndTalk : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                    // ����θ� Camera.main
    public float maxDistance = 3.0f;      // ��ȣ�ۿ� �Ÿ�
    public string elevatorTag = "Elevator";
    public LayerMask hitMask = ~0;        // ���� ���̾�(�⺻: Everything)

    [Header("Prompt UI ([F] ����)")]
    public TMP_Text promptText;           // ȭ�� �߾ӿ� ǥ���� TMP
    public string promptLine = "[F] ����";

    [Header("ĳ���� ���â (���� �� ���)")]
    public NarrationTextBarSafe characterBar; // �� �ϳ��� ���
    public Sprite characterIcon;              // ĳ���� ������
    [TextArea] public string talkLine = "����ִ°Ͱ���";
    public float talkHoldSeconds = 1.5f;

    [Header("Key")]
    public KeyCode useKey = KeyCode.F;

    bool lookingAtElevator = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (hitMask == 0) hitMask = ~0; // ��� ������ Everything
    }

    void Update()
    {
        UpdateAim();

        if (lookingAtElevator && Input.GetKeyDown(useKey))
        {
            if (characterBar)
            {
                // �� ���â GO�� ���� ������ �ڵ� Ȱ��
                if (!characterBar.gameObject.activeInHierarchy)
                    characterBar.gameObject.SetActive(true);
                // �� ������Ʈ�� ���� ������ �ѱ�
                if (!characterBar.enabled) characterBar.enabled = true;

                characterBar.ShowCharacter(talkLine, characterIcon, talkHoldSeconds);
            }
            else
            {
                Debug.LogWarning("[ElevatorAimPromptAndTalk] ĳ���� �ٰ� ������� �ʾҽ��ϴ�.");
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
                // �θ� ��Ʈ�� Elevator �±װ� ������ OK
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
