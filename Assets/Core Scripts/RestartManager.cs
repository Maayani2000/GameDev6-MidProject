using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private string sceneToLoad = "TheGame";
    [SerializeField] private bool reloadActiveScene = false;
    [SerializeField] private KeyCode mainMenuKey = KeyCode.M;

    static RestartManager instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        if (instance == null)
        {
            var go = new GameObject(nameof(RestartManager));
            instance = go.AddComponent<RestartManager>();
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && IsRestartAllowed(activeScene.name))
            {
                Restart();
            }
        }

        if (Input.GetKeyDown(mainMenuKey))
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.name == "Lose")
            {
                LoadMainMenu();
            }
        }
    }

    public void Restart()
    {
        if (!IsRestartAllowed(SceneManager.GetActiveScene().name)) return;

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    static bool IsRestartAllowed(string sceneName)
    {
        return sceneName == "Win" || sceneName == "Lose";
    }
}

