using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameReset
{
    static bool reloading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        reloading = false;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        reloading = false;
    }

    public static void ReloadCurrentScene()
    {
        if (reloading) return;
        reloading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
