using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node_PoliticalAxes : MonoBehaviour
{
    public void SyncPoliticalAxes(Node node)
    {
        userObj.transform.localPosition = node.userInformation.beliefs * (0.9f * 1.5f);

        if(connectedUserObjs.Count > 0)
        {
            for (int i = connectedUserObjs.Count - 1; i >= 0; i--)
            {
                Destroy(connectedUserObjs[i].gameObject);
                connectedUserObjs.RemoveAt(i);
            }
        }

        foreach(Node connectedNode in node.connectedNodes)
        {
            Debug.Log("I'm making a userIndicator representing " + connectedNode + " for " + node);
            var newUserObj = Instantiate(userObj, this.transform);
            newUserObj.transform.localScale *= 0.75f;
            newUserObj.transform.localPosition = connectedNode.userInformation.beliefs * (0.9f * 1.5f);
            
            if(connectedNode.isPlayer || connectedNode.isAlly)
            {
                newUserObj.GetComponent<Image>().color = Color.Lerp(Color.blue, Color.white, 0.5f);
            }
            else
            {
                newUserObj.GetComponent<Image>().color = Color.Lerp(Color.yellow, Color.white, 0.25f);
            }

            connectedUserObjs.Add(newUserObj);
        }
    }

    [SerializeField] private GameObject userObj;

    [SerializeField] private List<GameObject> connectedUserObjs = new List<GameObject>();
}
