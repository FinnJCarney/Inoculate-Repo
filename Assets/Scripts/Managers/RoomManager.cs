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
        smallScreenPivot.transform.DORotate(smallScreenPivotLocations[newGameState], timeForTransition);
        mainCamera.transform.DORotate(cameraLocations[newGameState].rotation.eulerAngles, timeForTransition);
        mainCamera.transform.DOMove(cameraLocations[newGameState].position, timeForTransition);
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

    public static RoomManager rM;
    private GameState gameState;
}
