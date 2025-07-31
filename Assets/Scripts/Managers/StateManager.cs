using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{

    private void Awake()
    {
        sM = this;
    }

    private void OnDestroy()
    {
        sM = null;
    }

    private void Start()
    {
        RoomManager.rM.AdjustVisualsToGameState(gameState, 0f);
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

        TimeManager.tM.SetTimeScale(0.02f);
    }

    private void Update()
    {
        if(LevelManager.lM != null)
        {
            gameOverCanvas.transform.position = LevelManager.lM.mapCamera.transform.position + gameOverCanvasLocalPos;
        }

        if (gameOver && Input.GetMouseButtonDown(0))
        {
            TimeManager.tM.SetTimeScale(1f);
            NodeManager.nM.FlushAllLinesAndNodes();
            ActionManager.aM.FlushAllActions();
            SceneLoader.sL.UnloadScene(LevelManager.lM.currentScene);
            if (wonOrLost)
            {
                LevelsToDisplay.Remove(LevelManager.lM.levelInfo);

                foreach(LevelInfo levelToAdd in LevelManager.lM.levelInfo.levelsToAdd)
                {
                    LevelsToDisplay.Add(levelToAdd);
                }

                foreach (LevelInfo levelToRemove in LevelManager.lM.levelInfo.levelsToRemove)
                {
                    LevelsToDisplay.Remove(levelToRemove);
                }
                StateManager.sM.gameState = GameState.LevelSelect;
                SceneLoader.sL.UnloadScene("InGameHUD");
                SceneLoader.sL.LoadSceneAdditive("LevelSelect");
            }
            else
            {
                SceneLoader.sL.LoadSceneAdditive(LevelManager.lM.levelInfo.LevelScene);
            }

            gameOver = false;
            SuccessScreen.SetActive(false);
            FailureScreen.SetActive(false);
            HUDManager.hM.SyncMenu(null);
            InputManager.iM.SetMCC(null);
            RoomManager.rM.AdjustVisualsToGameState(gameState, 1.5f);
        }
    }

    public void LoadLevelFromLevelSelect(LevelInfo levelInfo)
    {
        gameState = GameState.Mission;
        NodeManager.nM.FlushAllLinesAndNodes();
        ActionManager.aM.FlushAllActions();
        InputManager.iM.ClearCSV();  
        SceneLoader.sL.UnloadScene(LevelSelectManager.lsm.gameObject.scene.name);
        SceneLoader.sL.LoadSceneAdditive(levelInfo.LevelScene);
        SceneLoader.sL.LoadSceneAdditive("InGameHUD");
        RoomManager.rM.AdjustVisualsToGameState(gameState, 1.5f);
        TimeManager.tM.SetTimeScale(0.1f);
    }

    public static StateManager sM;

    private bool gameOver;
    private bool wonOrLost;
    [SerializeField] private GameObject SuccessScreen;
    [SerializeField] private GameObject FailureScreen;

    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private Vector3 gameOverCanvasLocalPos;

    public List<LevelInfo> LevelsToDisplay = new List<LevelInfo>();

    public GameState gameState;
}

public enum GameState
{ 
    Mission,
    LevelSelect,
    TitleScreen,
    None
}

