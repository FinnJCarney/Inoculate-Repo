using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TimeManager : MonoBehaviour
{

    private void Awake()
    {
        defaultTimeScale = 0.1f;
        desiredTimeScale = defaultTimeScale;

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
        Time.timeScale = defaultTimeScale + timeMultiplier;
        realTimeElapsed += Time.deltaTime / Time.timeScale;
        adjustedDeltaTime = Time.deltaTime * Time.timeScale;
    }

    public void SetTimeScale(float timeScale)
    {
        desiredTimeScale = timeScale;
        if(timeScaleTween != null)
        {
            timeScaleTween.Kill();
        }
        timeScaleTween.SetUpdate(true);
        timeScaleTween = DOTween.To(() => timeMultiplier, x => timeMultiplier = x, desiredTimeScale, 1f);
    }

    public void AddTimeScale(float timeScaleAddition)
    {
        desiredTimeScale += timeScaleAddition;
        if (timeScaleTween != null)
        {
            timeScaleTween.Kill();
        }
        timeScaleTween.SetUpdate(true);
        timeScaleTween = DOTween.To(() => timeMultiplier, x => timeMultiplier = x, desiredTimeScale, 2.5f);
    }

    public static TimeManager tM;

    Tween timeScaleTween;
    private float desiredTimeScale;
    private float defaultTimeScale;
    public float adjustedDeltaTime;
    [SerializeField][Range(0f, 16f)] public float timeMultiplier;
    public float realTimeElapsed;
}
