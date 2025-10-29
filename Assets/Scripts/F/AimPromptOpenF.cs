using UnityEngine;
using TMPro;

public class AimPromptOpenF : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                       // 비우면 Camera.main
    [Range(0.5f, 10f)] public float maxDistance = 3.0f;
    public LayerMask hitMask = ~0;           // 맞출 레이어(권장: Interactable만 체크)

    [Header("Match Rule (아래 중 원하는 것만 사용)")]
    public bool requireTag = true;
    public string targetTag = "Interactable";             // 태그로 판정
    public bool requireMinigameInteractable = false;      // 그 컴포넌트가 붙어 있어야 함

    [Header("UI")]
    public TMP_Text promptText;               // [F] 열기 텍스트
    public string promptLine = "[F] 열기";

    bool lookingAtTarget;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (hitMask == 0) hitMask = ~0; // 비어있으면 Everything
    }

    void Update()
    {
        lookingAtTarget = false;

        if (cam)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                // 부모/루트까지 포함해서 조건 검사
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
