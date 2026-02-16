using DG.Tweening;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class ConnectionLine : MonoBehaviour
{
    public List<NodeGroup> connectedNodeGroups = new List<NodeGroup>();

    private Faction myFaction;
    private Material myMaterial;
    private LineRenderer lineRenderer;
    Vector3 offset;

    [SerializeField] Material baseLineMaterial;

    public void Setup(Faction faction, NodeGroup nG1, NodeGroup nG2)
    {
        myFaction = faction;
        lineRenderer = GetComponent<LineRenderer>();
        myMaterial = new Material(baseLineMaterial);
        lineRenderer.material = myMaterial;

        connectedNodeGroups.Add(nG1);
        connectedNodeGroups.Add(nG2);

        //float offsetVal = Mathf.Clamp((Vector3.Distance(nG1.transform.position, nG2.transform.position) - 17f) / 2f, 0f, 18f);
        //float xSign = Mathf.Abs(nG2.transform.position.x) > Mathf.Abs(nG1.transform.position.x) ? Mathf.Sign(nG2.transform.position.x) : Mathf.Sign(nG1.transform.position.x);
        //float ySign = Mathf.Abs(nG2.transform.position.y) > Mathf.Abs(nG1.transform.position.y) ? Mathf.Sign(nG2.transform.position.y) : Mathf.Sign(nG1.transform.position.y);
        //float xOffset = nG1.transform.position.y == nG2.transform.position.y ? xSign * offsetVal : 0f;
        //float yOffset = nG1.transform.position.x == nG2.transform.position.x ? ySign * offsetVal : 0f;
        //if(yOffset != 0f && xOffset != 0f)
        //{
        //    yOffset *= 0.33f;
        //    xOffset *= 0.33f;
        //}
        //offset = new Vector3(xOffset, 0, yOffset);

        CheckOffset();
        LayoutLine(0);
        StartCoroutine(SyncLine(true));
    }

    private void CheckOffset()
    {
        float distanceCovered = Vector3.Distance(connectedNodeGroups[0].transform.position, connectedNodeGroups[1].transform.position);

        if(distanceCovered < 17f)
        {
            return;
        }

        Vector2 node1Pos = connectedNodeGroups[0].groupBelief;
        Vector2 node2Pos = connectedNodeGroups[1].groupBelief;


        int numOfPointsToCheck = Mathf.RoundToInt(distanceCovered / 14.5f);

        int numOfChecksToDo = 1;

        for(int i = 0; i < numOfChecksToDo; i++)
        {
            if (numOfChecksToDo > 64)
            {
                return;
            }

            for (int j = 0; j < numOfPointsToCheck; j++)
            {
                //Change this to iterate through points on line to find nearest nodeGroup (ignoring base nodeGroups)
                int pointToCheck = (j + 1 / numOfPointsToCheck + 2) * 25;
                Vector3 pointLocationV3 = Vector3.Lerp(connectedNodeGroups[0].transform.position, connectedNodeGroups[1].transform.position, pointToCheck / 25);

                float middleProximity = pointToCheck / 25f;
                pointLocationV3 += (offset * middleProximity);

                Vector2 pointLocation = new Vector2(pointLocationV3.x, pointLocationV3.z);

                Vector2 closestNodeGroupPos = new Vector2(Mathf.RoundToInt(pointLocation.x / 12f) * 12, Mathf.RoundToInt(pointLocation.y / 12f) * 12);

                if (!LevelManager.lM.nodeGroups.ContainsKey(closestNodeGroupPos) || LevelManager.lM.nodeGroups[closestNodeGroupPos].nodesInGroup.Count == 0)
                {
                    continue;
                }
                
                if(Vector2.Distance(pointLocation, closestNodeGroupPos) < 4f)
                {
                    numOfChecksToDo++;
                    float offsetVal = 3f;
                    float xDistance = Mathf.Abs(pointLocation.x -  closestNodeGroupPos.x);
                    float yDistance = Mathf.Abs(pointLocation.y - closestNodeGroupPos.y);
                    if(xDistance > yDistance)
                    {
                        float xSign = Mathf.Abs(pointLocation.x) > Mathf.Abs(pointLocation.x) ? Mathf.Sign(pointLocation.x) : Mathf.Sign(pointLocation.x);
                        offset.x += offsetVal * xSign;
                    }
                    else
                    {
                        float ySign = Mathf.Abs(pointLocation.y) > Mathf.Abs(pointLocation.y) ? Mathf.Sign(pointLocation.y) : Mathf.Sign(pointLocation.y);
                        offset.z += offsetVal * ySign;
                    }
                }
            }
        }
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
            pointLoc += (offset * middleProximity);
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
