using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadSceneAsync(1);
        SceneManager.LoadSceneAsync(2);
        SceneManager.LoadSceneAsync(3);
        SceneManager.UnloadScene(0);
    }
}
