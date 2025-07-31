using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    void Start()
    {
        foreach(string sceneToLoad in scenesToLoad)
        {
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
        }
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("Environment_Room"));
        SceneManager.UnloadSceneAsync("StartScene");
    }

    [SerializeField] string[] scenesToLoad;
}
