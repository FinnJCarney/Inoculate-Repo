using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    private void Start()
    {
        defaultTimeScale = 1.0f;
    }

    private void Update()
    {
        Time.timeScale = defaultTimeScale * timeMultiplier;
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    private float defaultTimeScale;
    [SerializeField][Range(0.1f, 8f)] float timeMultiplier;
}
