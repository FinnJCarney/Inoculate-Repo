using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numOfPointsOnLine;
        randomOffset = new Vector2(Random.Range(-randomOffsetRange, randomOffsetRange), Random.Range(-randomOffsetRange, randomOffsetRange));
    }

    public void SyncLine(float amountThrough, Vector3 startingPos, Vector3 endingPos, bool flip)
    {
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 newEndingPos = endingPos +  (Vector3.down * 0.5f);
            float positionValue = i / (float)lineRenderer.positionCount * amountThrough;
            float middleProximity = 1f - Mathf.Pow(Mathf.Abs((positionValue - 0.5f) * 2f), 2f);
            Vector3 pointPos = Vector3.Lerp(startingPos, newEndingPos, positionValue);
            if (!flip)
            {
                pointPos += (new Vector3(randomOffset.x, yValue, randomOffset.y)) * middleProximity;
            }
            else
            {
                pointPos += (new Vector3(randomOffset.x, -yValue, randomOffset.y)) * middleProximity;
            }
            lineRenderer.SetPosition(i, pointPos);
        }
    }

    private LineRenderer lineRenderer;
    private Vector2 randomOffset;

    [SerializeField] private int numOfPointsOnLine;
    [SerializeField] private float randomOffsetRange;
    [SerializeField] private float yValue;
}
