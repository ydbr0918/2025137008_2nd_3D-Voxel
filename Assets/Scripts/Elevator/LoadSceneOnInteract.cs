// LoadSceneOnInteract.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadSceneOnInteract : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Build Settings�� ��ϵ� �� �̸�")]
    public string sceneName;

    [Header("Interaction")]
    public string playerTag = "Player";
    public KeyCode useKey = KeyCode.F;

    [Header("Optional Prompt UI")]
    public TMP_Text promptText;                 // [F] ������ ��� TMP �ؽ�Ʈ(����)
    public string promptLine = "[F] �̵�";

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
                SceneManager.LoadScene(sceneName); // �� ��� �� ��ȯ
            else
                Debug.LogWarning("[LoadSceneOnInteract] sceneName�� ��� �ֽ��ϴ�.");
        }
    }
}
