using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    private void Awake()
    {
        defaultTimeScale = 1.0f;

        if(tM != null)
        {
            Destroy(this);
        }
        else
        {
            tM = this;
        }
    }

    private void OnDestroy()
    {
        tM = null;
    }

    private void Update()
    {
        Time.timeScale = defaultTimeScale * timeMultiplier;
        adjustedDeltaTime = Time.deltaTime * Time.timeScale;
    }

    public void SetTimeScale(float timeScale)
    {
        timeMultiplier = timeScale;
    }

    public static TimeManager tM;

    private float defaultTimeScale;
    public float adjustedDeltaTime;
    [SerializeField][Range(0f, 16f)] public float timeMultiplier;
}
