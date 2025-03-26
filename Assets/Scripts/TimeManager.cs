using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    private void Awake()
    {
        defaultTimeScale = 1.0f;

        if(i != null)
        {
            Destroy(this);
        }
        else
        {
            i = this;
        }
    }

    private void OnDestroy()
    {
        i = null;
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

    public static TimeManager i;

    private float defaultTimeScale;
    public float adjustedDeltaTime;
    [SerializeField][Range(0f, 8f)] public float timeMultiplier;
}
