using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{

    private void Start()
    {
        sM = this;
    }

    private void OnDestroy()
    {
        sM = null;
    }

    public void GameOver(bool won)
    {
        if(gameOver)
        {
            return;
        }

        gameOver = true;
        wonOrLost = won;

        if (won)
        {
            SuccessScreen.SetActive(true);
        }
        else
        {
            FailureScreen.SetActive(true);
        }

        TimeManager.tM.SetTimeScale(0.01f);
    }

    private void Update()
    {
        if(LevelManager.lM != null)
        {
            gameOverCanvas.transform.position = LevelManager.lM.mapCamera.transform.position + gameOverCanvasLocalPos;
        }

        if (gameOver && Input.GetMouseButtonDown(0))
        {
            NodeManager.nM.FlushAllLinesAndNodes();
            ActionManager.aM.FlushAllActions();
            SceneLoader.sL.UnloadScene(LevelManager.lM.currentScene);
            if (wonOrLost)
            {
                SceneLoader.sL.LoadSceneAdditive(LevelManager.lM.SuccessScene);
            }
            else
            {
                SceneLoader.sL.LoadSceneAdditive(LevelManager.lM.FailScene);
            }

            gameOver = false;
            SuccessScreen.SetActive(false);
            FailureScreen.SetActive(false);
            TimeManager.tM.SetTimeScale(1f);
            HUDManager.hM.SyncMenu(null);
        }
    }

    public static StateManager sM;

    private bool gameOver;
    private bool wonOrLost;
    [SerializeField] private GameObject SuccessScreen;
    [SerializeField] private GameObject FailureScreen;
    [SerializeField] private string failScene;
    [SerializeField] private string successScene;
    [SerializeField] private string currentScene;

    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private Vector3 gameOverCanvasLocalPos;
}
