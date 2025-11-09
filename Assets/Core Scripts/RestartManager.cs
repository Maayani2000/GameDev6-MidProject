using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private string sceneToLoad = "TheGame";
    [SerializeField] private bool reloadActiveScene = false;

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
    }

    public void Restart()
    {
        if (!IsRestartAllowed(SceneManager.GetActiveScene().name)) return;

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    static bool IsRestartAllowed(string sceneName)
    {
        return sceneName == "Win" || sceneName == "Lose";
    }
}

