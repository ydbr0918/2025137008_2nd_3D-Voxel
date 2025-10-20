using UnityEngine;
using TMPro;

public class PickupItem : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public KeyCode pickupKey = KeyCode.F;

    [Header("Prompt UI (선택)")]
    public CanvasGroup promptGroup;     // [F] 획득 패널 (CanvasGroup)
    public TMP_Text promptText;         // 예: "[F] 획득"
    public float promptFade = 0.2f;

    [Header("Attach On Pickup")]
    public Transform attachParent;      // 오른손 본(또는 소켓)
    public Vector3 localPos, localEuler, localScale = Vector3.one;

    System.Action _onPicked;
    bool _inside;

    public void SetOnPicked(System.Action onPicked) => _onPicked = onPicked;

    void Start()
    {
        if (promptGroup) { promptGroup.alpha = 0f; promptGroup.interactable = false; promptGroup.blocksRaycasts = false; }
        if (promptText && string.IsNullOrEmpty(promptText.text)) promptText.text = "[F] 획득";
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) { _inside = true; ShowPrompt(true); }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) { _inside = false; ShowPrompt(false); }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) { _inside = true; ShowPrompt(true); }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) { _inside = false; ShowPrompt(false); }
    }

    void Update()
    {
        if (!_inside) return;
        if (Input.GetKeyDown(pickupKey))
        {
            // 손에 붙이기
            if (attachParent)
            {
                transform.SetParent(attachParent);
                transform.localPosition = localPos;
                transform.localEulerAngles = localEuler;
                transform.localScale = localScale;
                // 충돌 비활성(옵션)
                foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
                var rb = GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;
            }
            ShowPrompt(false);
            _onPicked?.Invoke();
            // 아이템을 씬에서 없애고, 손에 든 복제본을 쓰려면 여기서 Destroy(gameObject) 대신 활성/비활성 로직으로 바꾸세요.
        }
    }

    async void ShowPrompt(bool on)
    {
        if (!promptGroup) return;
        float from = promptGroup.alpha, to = on ? 1f : 0f;
        float t = 0f;
        while (t < promptFade)
        {
            t += Time.deltaTime;
            promptGroup.alpha = Mathf.Lerp(from, to, t / promptFade);
            await System.Threading.Tasks.Task.Yield();
        }
        promptGroup.alpha = to;
    }
}
