using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(sL != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            sL = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LoadSceneAdditive(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void LoadSceneSingular(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void UnloadScene(string scene)
    {
        SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
    }

    public void FirstLoad()
    {
        LoadSceneSingular("TitleScreen");
        SceneManager.UnloadSceneAsync("StartScene");
    }

    public void LoadManagers()
    {
        LoadSceneAdditive("GameManagers");
    }

    public void LoadHUD()
    {
        LoadSceneAdditive("InGameHUD");
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public static SceneLoader sL;
}
