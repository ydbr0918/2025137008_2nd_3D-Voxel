using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "B5";          
    [SerializeField] private string tutorialSceneName = "Tutorial"; 

    // Start 버튼
    public void OnClickStart()
    {
        LoadSceneSafe(gameSceneName);
    }

    // Tutorial 버튼
    public void OnClickTutorial()
    {
        LoadSceneSafe(tutorialSceneName);
    }

    // Exit 버튼
    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
        Application.Quit(); // 빌드에서 앱 종료
#endif
    }

    // 씬 로드 공용 함수(오타/미등록 대비)
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
