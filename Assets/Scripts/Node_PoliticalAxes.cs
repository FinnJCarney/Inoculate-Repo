using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_PoliticalAxes : MonoBehaviour
{
    public void SyncPoliticalAxes(Node node)
    {
        userObj.transform.localPosition = node.userInformation.beliefs * 0.9f;
    }

    [SerializeField] private GameObject userObj;
}
