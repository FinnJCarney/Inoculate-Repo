using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    private void Awake()
    {
        rM = this;
    }

    private void Start()
    {
        SceneManager.SetActiveScene(this.gameObject.scene);
        phoneLocs[false] = phonePivot.transform.localPosition;
    }

    public void AdjustVisualsToGameState(GameState newGameState, float timeForTransition)
    {
        gameState = newGameState;
        //smallScreenPivot.transform.DORotate(smallScreenPivotLocations[newGameState], timeForTransition);
        mainCamera.transform.DORotate(cameraLocations[newGameState].rotation.eulerAngles, timeForTransition);
        DOTween.To(() => idealCamPos, x => idealCamPos = x, cameraLocations[newGameState].position, timeForTransition);

        if(newGameState == GameState.Mission)
        {
            smallScreenAnimator.SetInteger("RevealOrHide", 2);
        }
        else
        {
            Debug.Log("Is this being called");
            smallScreenAnimator.SetInteger("RevealOrHide", 1);
        }
    }

    public void LookAtPhone()
    {
        phonePivot.transform.DOKill();
        mainCamera.transform.DOKill();
        lookingAtPhone = !lookingAtPhone;
        if (lookingAtPhone)
        {
            phonePivot.transform.DOMove(phoneLocs[lookingAtPhone], 0.3f);
            //phonePivot.transform.DORotate(new Vector3(270f, 0f, -19.696f), 0.4f);
            mainCamera.transform.DORotate(phoneCamLoc.rotation.eulerAngles, 0.2f);
            mainCamera.transform.DOMove(phoneCamLoc.position, 0.25f);
            DOTween.To(() => idealCamPos, x => idealCamPos = x, phoneCamLoc.position, 0.25f);
        }
        else
        {
            phonePivot.transform.DOMove(phoneLocs[lookingAtPhone], 0.4f);
            //phonePivot.transform.DORotate(new Vector3(355f, 0f, -19.696f), 0.4f);
            AdjustVisualsToGameState(gameState, 0.33f);
        }
    }

    public void AdjustDonutHolder(int newNumOfActions)
    {
        donutHolder.UpdateDonuts(newNumOfActions);
    }

    public void Update()
    {
        //Vector2 newCamDriftVel = new Vector2(Random.Range(-maxCamDriftSpeed, maxCamDriftSpeed), Random.Range(-maxCamDriftSpeed, maxCamDriftSpeed));
        //newCamDriftVel += -camDriftPos * Mathf.Pow((camDriftPos.magnitude / camDriftRadius), 3f);

        //camDriftVel += newCamDriftVel;
        //camDriftPos = Vector2.Max(Vector2.Min(camDriftPos + (camDriftVel * Time.deltaTime), new Vector2(camDriftRadius, camDriftRadius)), new Vector2(-camDriftRadius, -camDriftRadius));
        float realTime = TimeManager.tM.realTimeElapsed * maxCamDriftSpeed;
        camDriftPos = new Vector2((Mathf.PerlinNoise(realTime, realTime + 1f) * 2f) - 1f, (Mathf.PerlinNoise(realTime + 1f, realTime) * 2f) - 1f) * camDriftRadius;
        mainCamera.transform.position = idealCamPos + new Vector3(camDriftPos.x, camDriftPos.y, 0);
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
    [SerializeField] private Animator smallScreenAnimator;
    [SerializeField] SerializableDictionary<GameState, Vector3> smallScreenPivotLocations = new SerializableDictionary<GameState, Vector3>();

    [SerializeField] private Camera mainCamera;
    private Vector3 idealCamPos;
    private Vector2 camDriftPos;
    [SerializeField] private float camDriftRadius;
    [SerializeField] private float maxCamDriftSpeed;
    [SerializeField] SerializableDictionary<GameState, Transform> cameraLocations = new SerializableDictionary<GameState, Transform>();

    private bool lookingAtPhone = false;
    [SerializeField] private GameObject phonePivot;
    [SerializeField] SerializableDictionary<bool, Vector3> phoneLocs = new SerializableDictionary<bool, Vector3>();
    [SerializeField] Transform phoneCamLoc; 

    public static RoomManager rM;
    private GameState gameState;

    [SerializeField] DonutHolder donutHolder;
}
