using DG.Tweening;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Xml;
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
    Vector3 randomOffset;

    [SerializeField] Material baseLineMaterial;

    public void Setup(Faction faction, NodeGroup nG1, NodeGroup nG2)
    {
        myFaction = faction;
        lineRenderer = GetComponent<LineRenderer>();
        myMaterial = new Material(baseLineMaterial);
        lineRenderer.material = myMaterial;

        connectedNodeGroups.Add(nG1);
        connectedNodeGroups.Add(nG2);

        float offsetVal = Mathf.Clamp((Vector3.Distance(nG1.transform.position, nG2.transform.position) - 17f) /3f, 0f, 12f);
        float xSign = Mathf.Abs(nG2.transform.position.x) > Mathf.Abs(nG1.transform.position.x) ? Mathf.Sign(nG2.transform.position.x) : Mathf.Sign(nG1.transform.position.x);
        float ySign = Mathf.Abs(nG2.transform.position.y) > Mathf.Abs(nG1.transform.position.y) ? Mathf.Sign(nG2.transform.position.y) : Mathf.Sign(nG1.transform.position.y);
        float xOffset = nG1.transform.position.y == nG2.transform.position.y ? xSign * offsetVal : 0f;
        float yOffset = nG1.transform.position.x == nG2.transform.position.x ? ySign * offsetVal : 0f;
        if(yOffset != 0f && xOffset != 0f)
        {
            yOffset *= 0.33f;
            xOffset *= 0.33f;
        }
        randomOffset = new Vector3(xOffset, 0, yOffset);

        LayoutLine(0);
        StartCoroutine(SyncLine(true));
    }

    private void LayoutLine(float progress)
    {
        int points = 25;
        lineRenderer.positionCount = points;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float pointPos = (i * 1f / points * 1f);
            if (i == lineRenderer.positionCount - 1) { pointPos = 1f; }
            pointPos -= 0.5f;
            float lineProgress = 0.5f - (pointPos * progress);
            float middleProximity = 1f - Mathf.Pow(Mathf.Abs((lineProgress - 0.5f) * 2f), 2f);
            Vector3 pointLoc = Vector3.Lerp(connectedNodeGroups[0].transform.position, connectedNodeGroups[1].transform.position, lineProgress);
            pointLoc += (randomOffset * middleProximity);
            lineRenderer.SetPosition(i, pointLoc);
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
