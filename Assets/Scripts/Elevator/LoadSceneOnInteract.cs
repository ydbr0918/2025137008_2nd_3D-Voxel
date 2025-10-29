// LoadSceneOnInteract.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadSceneOnInteract : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Build Settings에 등록된 씬 이름")]
    public string sceneName;

    [Header("Interaction")]
    public string playerTag = "Player";
    public KeyCode useKey = KeyCode.F;

    [Header("Optional Prompt UI")]
    public TMP_Text promptText;                 // [F] 문구를 띄울 TMP 텍스트(선택)
    public string promptLine = "[F] 이동";

    bool playerInside;

    void Awake()
    {
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        if (promptText)
        {
            promptText.text = promptLine;
            promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(useKey))
        {
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName); // ★ 즉시 씬 전환
            else
                Debug.LogWarning("[LoadSceneOnInteract] sceneName이 비어 있습니다.");
        }
    }
}
