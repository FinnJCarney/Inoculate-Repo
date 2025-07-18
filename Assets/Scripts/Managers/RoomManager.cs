using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    private void Awake()
    {
        rM = this;
    }

    public void AdjustVisualsToGameState(GameState newGameState, float timeForTransition)
    {
        gameState = newGameState;
        smallScreenPivot.transform.DORotate(smallScreenPivotLocations[newGameState], timeForTransition);
        mainCamera.transform.DORotate(cameraLocations[newGameState].rotation.eulerAngles, timeForTransition);
        mainCamera.transform.DOMove(cameraLocations[newGameState].position, timeForTransition);
    }

    public void LookAtPhone()
    {
        phonePivot.transform.DOKill();
        mainCamera.transform.DOKill();
        lookingAtPhone = !lookingAtPhone;
        if (lookingAtPhone)
        {
            phonePivot.transform.DOMove(phoneLocs[lookingAtPhone], 0.33f);
            mainCamera.transform.DORotate(phoneCamLoc.rotation.eulerAngles, 0.25f);
            mainCamera.transform.DOMove(phoneCamLoc.position, 0.25f);
        }
        else
        {
            phonePivot.transform.DOMove(phoneLocs[lookingAtPhone], 0.8f);
            AdjustVisualsToGameState(gameState, 0.33f);
        }
    }
    
    //IEnumerator MoveSmallScreen(GameState newGameState)
    //{
    //    if(smallScreenPivot.transform.rotation.eulerAngles == smallScreenPivotLocations[newGameState])
    //    {
    //        return null;
    //    }
    //
    //    float amountThrough = 0f;
    //    while (amountThrough < 1f)
    //    {
    //        yield return null;
    //    }
    //    yield return new WaitForSeconds(0.5f);
    //}

    [SerializeField] private GameObject smallScreenPivot;
    [SerializeField] SerializableDictionary<GameState, Vector3> smallScreenPivotLocations = new SerializableDictionary<GameState, Vector3>();

    [SerializeField] private Camera mainCamera;
    [SerializeField] SerializableDictionary<GameState, Transform> cameraLocations = new SerializableDictionary<GameState, Transform>();

    private bool lookingAtPhone = false;
    [SerializeField] private GameObject phonePivot;
    [SerializeField] SerializableDictionary<bool, Vector3> phoneLocs = new SerializableDictionary<bool, Vector3>();
    [SerializeField] Transform phoneCamLoc; 

    public static RoomManager rM;
    private GameState gameState;
}
