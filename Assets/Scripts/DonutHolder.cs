using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DonutHolder : MonoBehaviour
{
    private void Start()
    {
        for (int i = 0; i < donutMRs.Length; i++)
        {
            donutMats[i] = new Material(donut_Off);
            donutMRs[i].material = donutMats[i];
            donutEmCols[i] = donutMats[i].GetColor("_EmissionColor");
        }
    }

    public void UpdateDonuts(int newNumOfActions)
    {
        StopCoroutine(UpdateDonutColors());

        for (int i = 0; i < donutMats.Length; i++)
        {
            if (newNumOfActions - 1 > i - 0.1f)
            {
                donutEmCols[i] = donut_On.GetColor("_EmissionColor");
            }

            else
            {
                donutEmCols[i] = donut_Off.GetColor("_EmissionColor");
            }
        }

        StartCoroutine(UpdateDonutColors());
    }

    private IEnumerator UpdateDonutColors()
    {
        for (float t = 0; t < 0.33f; t += Time.unscaledDeltaTime)
        {
            for (int i = 0; i < donutMats.Length; i++)
            {
                donutMats[i].SetColor("_EmissionColor", Color.Lerp(donutMats[i].GetColor("_EmissionColor"), donutEmCols[i], t / 0.33f));

                yield return null;
            }
        }
    }

    //private void Update()
    //{
    //    for (int i = 0; i < donutMats.Length; i++)
    //    {
    //        donutMats[i].SetColor("_EmissionColor", new Color(donutEmCols[i].r, donutEmCols[i].g, donutEmCols[i].b));
    //    }
    //}

    [SerializeField] MeshRenderer[] donutMRs;
    [SerializeField] private Material[] donutMats;
    [SerializeField] private Color[] donutEmCols;

    [SerializeField] Material donut_Off;
    [SerializeField] Material donut_On;

    private Dictionary<Material, Tween> MaterialTweens = new Dictionary<Material, Tween>();
}
