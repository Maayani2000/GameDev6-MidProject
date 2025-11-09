using UnityEngine;
using UnityEngine.SceneManagement;

public class AnyKeyStart : MonoBehaviour
{
    [SerializeField] private string sceneName = "TheGame";
    [SerializeField] private KeyCode[] ignoreKeys = { };
    bool hasStarted = false;

    void Update()
    {
        if (hasStarted) return;

        if (Input.anyKeyDown)
        {
            if (ignoreKeys != null)
            {
                foreach (var key in ignoreKeys)
                {
                    if (Input.GetKeyDown(key))
                        return;
                }
            }

            hasStarted = true;
            SceneManager.LoadScene(sceneName);
        }
    }
}

