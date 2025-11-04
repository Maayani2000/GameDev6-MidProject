using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void StartGame() // Load the main game scene (map)
    {
        //SceneManager.LoadScene("TheGame");
        SceneManager.LoadScene("Sample 1");
    }

    public void TryAgain() // Reload the main game scene (reset the map)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu() // load the main menu (title & start)
    {
        SceneManager.LoadScene("MainMenu"); // Replace with your actual main menu scene name
    }

    public void QuitGame() // Quit > Pause the play mode if in editor or quit the game in buid back to windows.
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
