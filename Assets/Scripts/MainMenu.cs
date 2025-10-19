using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "B5";          
    [SerializeField] private string tutorialSceneName = "Tutorial"; 

    // Start ��ư
    public void OnClickStart()
    {
        LoadSceneSafe(gameSceneName);
    }

    // Tutorial ��ư
    public void OnClickTutorial()
    {
        LoadSceneSafe(tutorialSceneName);
    }

    // Exit ��ư
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit(); // ���忡�� �� ����
#endif
    }

    // �� �ε� ���� �Լ�(��Ÿ/�̵�� ���)
    private void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[MainMenu] Scene name is empty.");
            return;
        }

        
        SceneManager.LoadScene(sceneName);
    }
}
