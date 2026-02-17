using UnityEngine;

public class ExecuteActionButtonEnabler : MonoBehaviour
{
    [SerializeField] private GameObject buttonToEnable;

    void Start()
    {
        EventManager.ChangeStateEvent += EnableButton;
    }

    void EnableButton(LevelState newState)
    {
        if(newState == LevelState.Planning)
        {
            buttonToEnable.SetActive(true);
        }
        else
        {
            buttonToEnable.SetActive(false);
        }
    }
}
