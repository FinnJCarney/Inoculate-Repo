using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TimeManager : MonoBehaviour
{

    private void Awake()
    {
        defaultTimeScale = 0.0f;
        desiredTimeScale = defaultTimeScale;

        if (tM != null)
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
        realTimeElapsed += Time.unscaledDeltaTime;
        gameTimeElapsed += Time.deltaTime;
    }

    public void SetTimeScale(float timeScale)
    {
        desiredTimeScale = timeScale;

        if (desiredTimeScale == timeMultiplier)
        {
            return;
        }

        if (timeScaleTween != null)
        {
            timeScaleTween.Kill();
        }
        timeScaleTween.SetUpdate(true);
        timeScaleTween = DOTween.To(() => timeMultiplier, x => timeMultiplier = x, desiredTimeScale, 1f);
    }

    public static TimeManager tM;

    Tween timeScaleTween;
    private float desiredTimeScale;
    private float defaultTimeScale;
    [SerializeField][Range(0f, 16f)] public float timeMultiplier;
    public float realTimeElapsed;
    public float gameTimeElapsed;
}
