using UnityEngine;
using UnityEngine.SceneManagement;

public static class InitialSceneLoader
{
    private const string MainMenuSceneName = "MainMenu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureMainMenuIsLoaded()
    {
        var activeScene = SceneManager.GetActiveScene();

        if (!activeScene.IsValid() || activeScene.name != MainMenuSceneName)
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}

