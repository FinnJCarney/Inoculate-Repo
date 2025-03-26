using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Node_PoliticalAxes : MonoBehaviour
{
    public void SyncPoliticalAxes(Node node)
    {
        if (!node.userInformation.userInfoHidden)
        {
            if(myUserObj == null)
            {
                myUserObj = Instantiate(userObj, userObjsHolder);
                Vector3 defaultScale = myUserObj.transform.localScale;
                myUserObj.transform.localScale = new Vector3(0, defaultScale.y, defaultScale.z);
                myUserObj.transform.localPosition = node.userInformation.beliefs * (0.9f * 1.5f);
                myUserObj.transform.DOScale(defaultScale, 0.5f);

                if (node.showMenu)
                {
                    myUserObj.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
                    myUserObj.GetComponent<AudioSource>().PlayOneShot(userReveal);
                }
            }

            if (myUserObj.transform.localPosition.x != node.userInformation.beliefs.x * (0.9f * 1.5f) || myUserObj.transform.localPosition.y != node.userInformation.beliefs.y * (0.9f * 1.5f))
            {
                if (node.showMenu)
                {
                    myUserObj.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
                    myUserObj.GetComponent<AudioSource>().PlayOneShot(userMove);
                }
                myUserObj.transform.DOLocalMove(node.userInformation.beliefs * (0.9f * 1.5f), 0.3f);
            }
        }

        foreach (Node connectedNode in node.connectedNodes)
        {
            if (!connectedNode.userInformation.userInfoHidden)
            {
                connectedUserObj associatedUserObj = new connectedUserObj();

                foreach(connectedUserObj cUO in connectedUserObjs)
                {
                    if(connectedNode == cUO.asociatedNode)
                    {
                        associatedUserObj = cUO;
                        break;
                    }
                }

                if (associatedUserObj.userObj == null)
                {
                    associatedUserObj.userObj = Instantiate(userObj, userObjsHolder);
                    Vector3 defaultScale = associatedUserObj.userObj.transform.localScale * 0.75f;
                    associatedUserObj.userObj.transform.localScale = new Vector3(0, defaultScale.y, defaultScale.z);
                    associatedUserObj.userObj.transform.localPosition = connectedNode.userInformation.beliefs * (0.9f * 1.5f);
                    associatedUserObj.userObj.transform.DOScale(defaultScale, 0.5f);
                    associatedUserObj.userImage = associatedUserObj.userObj.GetComponent<Image>();
                    associatedUserObj.userAudioSource = associatedUserObj.userObj.GetComponent<AudioSource>();
                    associatedUserObj.asociatedNode = connectedNode;
                    connectedUserObjs.Add(associatedUserObj);

                    if (node.showMenu)
                    {
                        associatedUserObj.userAudioSource.pitch = Random.Range(0.9f, 1.1f);
                        associatedUserObj.userAudioSource.PlayOneShot(userReveal);
                    }
                }

                if (connectedNode.isPlayer || connectedNode.isAlly)
                {
                    associatedUserObj.userImage.color = Color.Lerp(Color.blue, Color.white, 0.5f);
                }
                else
                {
                    associatedUserObj.userImage.color = Color.Lerp(Color.yellow, Color.white, 0.125f);
                }

                if (associatedUserObj.userObj.transform.localPosition.x != connectedNode.userInformation.beliefs.x * (0.9f * 1.5f) || associatedUserObj.userObj.transform.localPosition.y != connectedNode.userInformation.beliefs.y * (0.9f * 1.5f))
                {
                    if (node.showMenu)
                    {
                        associatedUserObj.userAudioSource.pitch = Random.Range(0.9f, 1.1f);
                        associatedUserObj.userAudioSource.PlayOneShot(userMove);
                    }
                    associatedUserObj.userObj.transform.DOLocalMove(connectedNode.userInformation.beliefs * (0.9f * 1.5f), 0.3f);
                }

            }
        }

        if (connectedUserObjs.Count > 0)
        {
            for (int i = connectedUserObjs.Count - 1; i >= 0; i--)
            {
                bool associatedNodeExists = false;

                foreach (Node connectedNode in node.connectedNodes)
                { 
                    if(connectedNode == connectedUserObjs[i].asociatedNode)
                    {
                        associatedNodeExists = true;
                    }
                }

                if(!associatedNodeExists)
                {
                    Destroy(connectedUserObjs[i].userObj);
                    connectedUserObjs.RemoveAt(i);
                }
            }
        }
    }

    [SerializeField] private GameObject userObj;
    [SerializeField] private Transform userObjsHolder;

    [SerializeField] private GameObject myUserObj;
    [SerializeField] private List<connectedUserObj> connectedUserObjs = new List<connectedUserObj>();

    [SerializeField] private AudioClip userReveal;
    [SerializeField] private AudioClip userMove;

    [System.Serializable]
    public struct connectedUserObj
    { 
        public GameObject userObj;
        public Image userImage;
        public AudioSource userAudioSource;
        public Node asociatedNode;
    }
}
