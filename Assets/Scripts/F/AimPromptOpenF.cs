using UnityEngine;
using TMPro;

public class AimPromptOpenF : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                       // ���� Camera.main
    [Range(0.5f, 10f)] public float maxDistance = 3.0f;
    public LayerMask hitMask = ~0;           // ���� ���̾�(����: Interactable�� üũ)

    [Header("Match Rule (�Ʒ� �� ���ϴ� �͸� ���)")]
    public bool requireTag = true;
    public string targetTag = "Interactable";             // �±׷� ����
    public bool requireMinigameInteractable = false;      // �� ������Ʈ�� �پ� �־�� ��

    [Header("UI")]
    public TMP_Text promptText;               // [F] ���� �ؽ�Ʈ
    public string promptLine = "[F] ����";

    bool lookingAtTarget;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (hitMask == 0) hitMask = ~0; // ��������� Everything
    }

    void Update()
    {
        lookingAtTarget = false;

        if (cam)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                // �θ�/��Ʈ���� �����ؼ� ���� �˻�
                Transform t = hit.collider.transform;

                bool ok = true;

                if (requireTag)
                {
                    ok &= (t.CompareTag(targetTag) ||
                          (t.parent && t.parent.CompareTag(targetTag)) ||
                          t.root.CompareTag(targetTag));
                }

                if (requireMinigameInteractable)
                {
                    ok &= (hit.collider.GetComponentInParent<MinigameInteractable>() != null);
                }

                lookingAtTarget = ok;
            }
        }

        if (promptText)
        {
            promptText.gameObject.SetActive(lookingAtTarget);
            if (lookingAtTarget) promptText.text = promptLine;
        }
    }

    void OnDisable()
    {
        if (promptText) promptText.gameObject.SetActive(false);
    }
}
