using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{

    private void Start()
    {
        sM = this;
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

        timeManager.SetTimeScale(0.01f);
    }

    private void Update()
    {
        if(gameOver && Input.GetMouseButtonDown(0))
        {
            if (wonOrLost)
            {
                SceneLoader.sL.LoadScene(successScene);
            }
            else
            {
                SceneLoader.sL.LoadScene(failScene);
            }
        }
    }

    public static StateManager sM;

    private bool gameOver;
    private bool wonOrLost;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private GameObject SuccessScreen;
    [SerializeField] private GameObject FailureScreen;
    [SerializeField] private string failScene;
    [SerializeField] private string successScene;
}
