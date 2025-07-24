using CRTFilter;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using UnityEngine.Rendering;
using Unity.Mathematics;

public class VisualsManager : MonoBehaviour
{

    private void Start()
    {
        vM = this;

        var renderFeature = uRD.rendererFeatures[0];
        cRTRF = renderFeature.ConvertTo<CRTRendererFeature>();
        defaultShadowLines = cRTRF.shadowlines;
        defaultShadowLinesSpeed = cRTRF.shadowlinesSpeed;
        defaultVignetteSmooth = cRTRF.vignetteSmooth;
        defaultBleed = cRTRF.bleed;

        var vProf = globalVolume.profile;
        vProf.TryGet<Bloom>(out bloom);
        vProf.TryGet<FilmGrain>(out filmGrain);
        defaultBloomIntensity = bloom.intensity.value;
        defaultFilmGrainIntensity = filmGrain.intensity.value;
    }

    public void SwapLayer(bool online)
    {
        var vProf = globalVolume.profile;
        var psMain = offlinePS.main;

        if (online)
        {
            ScreenPlane.bigScreen.cameraCursor.camera.DOColor(onlineBackground, 0.5f);
            DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, defaultBloomIntensity, 0.5f);
            DOTween.To(() => filmGrain.intensity.value, x => filmGrain.intensity.value = x, defaultFilmGrainIntensity, 0.5f);
            DOTween.To(() => cRTRF.bleed, x => cRTRF.bleed = x, defaultBleed, 0.5f);
            DOTween.To(() => cRTRF.shadowlines, x => cRTRF.shadowlines = x, defaultShadowLines, 0.5f);
            DOTween.To(() => cRTRF.shadowlinesSpeed, x => cRTRF.shadowlinesSpeed = x, defaultShadowLinesSpeed, 0.5f);
            DOTween.To(() => cRTRF.vignetteSmooth, x => cRTRF.vignetteSmooth = x, defaultVignetteSmooth, 0.5f);
            offlinePSMat.DOColor(Color.clear, 0.5f);
            
        }
        else
        {
            ScreenPlane.bigScreen.cameraCursor.camera.DOColor(offlineBackground, 0.5f);
            DOTween.To(() => bloom.intensity.value, x => bloom.intensity.value = x, 1.25f, 0.5f);
            DOTween.To(() => filmGrain.intensity.value, x => filmGrain.intensity.value = x, 0f, 0.5f);
            DOTween.To(() => cRTRF.bleed, x => cRTRF.bleed = x, 0f, 0.5f);
            DOTween.To(() => cRTRF.shadowlines, x => cRTRF.shadowlines = x, 0f, 0.5f);
            DOTween.To(() => cRTRF.shadowlinesSpeed, x => cRTRF.shadowlinesSpeed = x, 0, 0.5f);
            DOTween.To(() => cRTRF.vignetteSmooth, x => cRTRF.vignetteSmooth = x, 0.025f, 0.5f);
            offlinePSMat.DOColor(offlinePSColor, 0.5f);
        }
    }

    public static VisualsManager vM;

    [SerializeField] private UniversalRendererData uRD;
    private CRTRendererFeature cRTRF;
    [SerializeField] private Volume globalVolume;
    private Bloom bloom;
    private FilmGrain filmGrain;
    [SerializeField] ParticleSystem offlinePS;
    [SerializeField] private Material offlinePSMat;
    [SerializeField] private Color offlinePSColor;

    [SerializeField] Color onlineBackground;
    [SerializeField] Color offlineBackground;

    private float defaultBloomIntensity;
    private float defaultFilmGrainIntensity;

    private float defaultShadowLines;
    private float defaultShadowLinesSpeed; 
    private float defaultVignetteSmooth;
    private float defaultBleed;

    [SerializeField] private bool changeLayer;
}
