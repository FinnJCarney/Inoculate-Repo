using DG.Tweening;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ConnectionLine : MonoBehaviour
{
    public List<NodeGroup> connectedNodeGroups = new List<NodeGroup>();

    private Faction myFaction;
    private Material myMaterial;
    private LineRenderer lineRenderer;

    [SerializeField] Material baseLineMaterial;

    public void Setup(Faction faction, NodeGroup nG1, NodeGroup nG2)
    {
        myFaction = faction;
        lineRenderer = GetComponent<LineRenderer>();
        myMaterial = new Material(baseLineMaterial);
        lineRenderer.material = myMaterial;

        connectedNodeGroups.Add(nG1);
        connectedNodeGroups.Add(nG2);

        LayoutLine(0);
        StartCoroutine(SyncLine(true));
    }

    private void LayoutLine(float progress)
    {
        int points = 10;
        lineRenderer.positionCount = points;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float pointPos = (i * 1f / points * 1f);
            if (i == lineRenderer.positionCount - 1) { pointPos = 1f; }
            pointPos -= 0.5f;
            float lineProgress = 0.5f - (pointPos * progress);
            lineRenderer.SetPosition(i, Vector3.Lerp(connectedNodeGroups[0].transform.position, connectedNodeGroups[1].transform.position, lineProgress));
        }
    }

    public void Adjust(bool show, Faction faction)
    {
        if (myFaction != faction || !show)
        {
            myFaction = faction;
            StopCoroutine(SyncLine(show));
            StartCoroutine(SyncLine(show));
        }
    }

    private IEnumerator SyncLine(bool show)
    {
        Color desiredColor = show ? LevelManager.lM.levelFactions[myFaction].color * 1.25f : Color.clear;

        Color currentColor = lineRenderer.startColor;

        float timerEnd = show ? 0.5f : 0.5f;
        float adjustment = show ? 2f : 2f;

        for(float timer = 0;  timer < timerEnd; timer += Time.unscaledDeltaTime)
        {
            float timerProgress = timer / timerEnd;
            timerProgress = Mathf.Pow(timerProgress, adjustment);
            lineRenderer.startColor = Color.Lerp(currentColor, desiredColor, timerProgress);
            lineRenderer.endColor = Color.Lerp(currentColor, desiredColor, timerProgress);

            if (show)
            {
                LayoutLine(timerProgress);
            }

            else
            {
                LayoutLine(1 - timerProgress);
            }

            yield return null;
        }

        lineRenderer.startColor = desiredColor;
        lineRenderer.endColor = desiredColor;

        if (show)
        {
            LayoutLine(1f);
        }

        if (!show)
        {
            Destroy(myMaterial);
            Destroy(this.gameObject);
        }
    }

}
