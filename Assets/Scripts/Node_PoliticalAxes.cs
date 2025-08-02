using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq.Expressions;

public class Node_PoliticalAxes : MonoBehaviour
{
    public void SyncPoliticalAxes(Node node, bool onClick)
    {
        if (crIsRunning)
        {
            StopCoroutine("ReCheckLines");
            crIsRunning = false;
        }

        if (node == null)
        {
            return;
        }

        if(node == prevNode && onClick)
        {
            return;
        }

        if(node.userInformation.userInfoHidden)
        {
            ClearAxes();
        }

        nodeObjsOnBoard.Clear();

        if (!node.userInformation.userInfoHidden)
        {
            if (myUserObj == null)
            {
                myUserObj = Instantiate(userObj, userObjsHolder);
                myUserObj.GetComponent<UserIndicator>().node = node;

                if (!onClick)
                {
                    Vector3 defaultScale = myUserObj.transform.localScale * 1.25f;
                    myUserObj.transform.localScale = new Vector3(0, defaultScale.y, defaultScale.z);
                    myUserObj.transform.DOScale(defaultScale, 0.5f);

                    myUserObj.transform.localPosition = node.userInformation.beliefs * 1.4f;

                    myUserObj.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
                    myUserObj.GetComponent<AudioSource>().PlayOneShot(userReveal);

                }
                else
                {
                    myUserObj.transform.localPosition = node.userInformation.beliefs * 1.4f;
                }
            }

            objOnBoard userObjOnBoard = new objOnBoard();
            userObjOnBoard.userObj = myUserObj;
            userObjOnBoard.userImage = myUserObj.GetComponent<UserIndicator>().profile;
            userObjOnBoard.userFaction = myUserObj.GetComponent<UserIndicator>().faction;
            userObjOnBoard.userAudioSource = myUserObj.GetComponent<AudioSource>();
            userObjOnBoard.associatedNode = node;
            userObjOnBoard.userImage.sprite = userObjOnBoard.associatedNode.nodeVisual.sprite;
            userObjOnBoard.userImage.color = Color.Lerp(LevelManager.lM.GiveAverageColor(node.userInformation.beliefs), Color.white, 0.5f);
            userObjOnBoard.userFaction.color = LevelManager.lM.levelFactions[node.userInformation.faction].color;
            nodeObjsOnBoard.Add(userObjOnBoard);

            if (myUserObj.transform.localPosition.x != node.userInformation.beliefs.x * 1.4f || myUserObj.transform.localPosition.y != node.userInformation.beliefs.y * 1.4f)
            {
                if (!onClick)
                {
                    myUserObj.transform.DOLocalMove(node.userInformation.beliefs * 1.4f, 0.3f);
                    myUserObj.GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);
                    myUserObj.GetComponent<AudioSource>().PlayOneShot(userMove);
                }
                else
                {
                    myUserObj.transform.localPosition = node.userInformation.beliefs * 1.4f;
                }
            }

            foreach (Node connectedNode in node.userInformation.connectedNodes.Keys)
            {
                if (!connectedNode.userInformation.userInfoHidden)
                {
                    objOnBoard associatedUserObj = new objOnBoard();

                    foreach (objOnBoard cUO in connectedUserObjs)
                    {
                        if (connectedNode == cUO.associatedNode)
                        {
                            associatedUserObj = cUO;
                            break;
                        }
                    }

                    if (associatedUserObj.userObj == null)
                    {
                        associatedUserObj.userObj = Instantiate(userObj, userObjsHolder);
                        associatedUserObj.userImage = associatedUserObj.userObj.GetComponent<UserIndicator>().profile;
                        associatedUserObj.userFaction = associatedUserObj.userObj.GetComponent<UserIndicator>().faction;
                        associatedUserObj.userObj.GetComponent<UserIndicator>().node = connectedNode;
                        associatedUserObj.userAudioSource = associatedUserObj.userObj.GetComponent<AudioSource>();
                        associatedUserObj.associatedNode = connectedNode;
                        associatedUserObj.userImage.sprite = connectedNode.nodeVisual.sprite;
                        connectedUserObjs.Add(associatedUserObj);

                        if (!onClick)
                        {
                            Vector3 defaultScale = associatedUserObj.userObj.transform.localScale * 0.6f;
                            associatedUserObj.userObj.transform.localScale = new Vector3(0, defaultScale.y, defaultScale.z);
                            associatedUserObj.userObj.transform.DOScale(defaultScale, 0.5f);

                            associatedUserObj.userObj.transform.localPosition = connectedNode.userInformation.beliefs * 1.4f;

                            associatedUserObj.userAudioSource.pitch = Random.Range(0.9f, 1.1f);
                            associatedUserObj.userAudioSource.PlayOneShot(userReveal);
                        }
                        else
                        {
                            associatedUserObj.userObj.transform.localScale *= 0.6f;
                            associatedUserObj.userObj.transform.localPosition = connectedNode.userInformation.beliefs * 1.4f;
                        }
                    }

                    nodeObjsOnBoard.Add(associatedUserObj);

                    associatedUserObj.userImage.color = Color.Lerp(LevelManager.lM.GiveAverageColor(associatedUserObj.associatedNode.userInformation.beliefs), Color.white, 0.5f);
                    associatedUserObj.userFaction.color = LevelManager.lM.levelFactions[associatedUserObj.associatedNode.userInformation.faction].color;

                    if (associatedUserObj.userObj.transform.localPosition.x != connectedNode.userInformation.beliefs.x * 1.4f || associatedUserObj.userObj.transform.localPosition.y != connectedNode.userInformation.beliefs.y * 1.4f)
                    {
                        if (!onClick)
                        {
                            associatedUserObj.userObj.transform.DOLocalMove(connectedNode.userInformation.beliefs * 1.4f, 0.3f);
                            associatedUserObj.userAudioSource.pitch = Random.Range(0.9f, 1.1f);
                            associatedUserObj.userAudioSource.PlayOneShot(userMove);
                        }
                        else
                        {
                            associatedUserObj.userObj.transform.localPosition = connectedNode.userInformation.beliefs * 1.4f;
                        }
                    }
                }
            }

            StartCoroutine("ReCheckLines");

            if (connectedUserObjs.Count > 0)
            {
                for (int i = connectedUserObjs.Count - 1; i >= 0; i--)
                {
                    bool associatedNodeExists = false;

                    foreach (Node connectedNode in node.userInformation.connectedNodes.Keys)
                    {
                        if (connectedNode == connectedUserObjs[i].associatedNode)
                        {
                            associatedNodeExists = true;
                        }
                    }

                    if (!associatedNodeExists)
                    {
                        if(nodeObjsOnBoard.Contains(connectedUserObjs[i]))
                        {
                            nodeObjsOnBoard.Remove(connectedUserObjs[i]);
                        }
                        Destroy(connectedUserObjs[i].userObj);
                        connectedUserObjs.RemoveAt(i);
                    }
                }
            }

            for (int i = hudLines.Count - 1; i >= 0; i--)
            {
                Destroy(hudLines[i].lineObj);
                hudLines.Remove(hudLines[i]);
            }

            List<Node> nodesOnBoard = new List<Node>();

            foreach(objOnBoard objOB in nodeObjsOnBoard)
            {
                nodesOnBoard.Add(objOB.associatedNode);
            }

            foreach (Line line in NodeManager.nM.lines)
            {
                //Is the line associated with a faction
                if(line.lineFaction == Faction.Neutral)
                {
                    continue;
                }

                //Is one of the nodes it on the HUD
                if (!nodesOnBoard.Contains(line.connectedNodes[0]))
                {
                    continue;
                }
                
                //Is the other node in it on the HUD (and also not the same node as node 1)
                if (!nodesOnBoard.Contains(line.connectedNodes[1]))
                {
                    continue;
                }

                bool breakOutForLoop = false;

                //Is this connection already represented by a line
                foreach (HUDLines hudLine in hudLines)
                {
                    if (hudLine.containingNodes.Contains(line.connectedNodes[0]) && hudLine.containingNodes.Contains(line.connectedNodes[1]))
                    {
                        breakOutForLoop = true;
                        continue;
                    }
                }

                if (breakOutForLoop)
                {
                    continue;
                }

                if (line.connectedNodes[0].userInformation.beliefs == line.connectedNodes[1].userInformation.beliefs)
                {
                    continue;
                }

                GameObject nodeObj1 = null;
                GameObject nodeObj2 = null;

                foreach (objOnBoard objOB in nodeObjsOnBoard)
                {
                    if(objOB.associatedNode == line.connectedNodes[0])
                    {
                        nodeObj1 = objOB.userObj;
                    }

                    if (objOB.associatedNode == line.connectedNodes[1])
                    {
                        nodeObj2 = objOB.userObj;
                    }
                }

                var newLine = Instantiate(HUDLineObj, HUDLineHolder.transform);
                newLine.transform.localPosition = Vector3.Lerp(nodeObj1.transform.localPosition, nodeObj2.transform.localPosition, 0.5f);
                newLine.transform.localPosition += Vector3.forward * 0.25f;

                if (nodeObj1.transform.localPosition.y == nodeObj2.transform.localPosition.y)
                {
                    newLine.transform.Rotate(0, 0, 90);
                }

                newLine.GetComponent<Image>().material = LevelManager.lM.GiveLineMaterial(line.lineFaction);
                var newHudLine = new HUDLines();
                newHudLine.lineObj = newLine;
                newHudLine.containingNodes = new List<Node>();
                newHudLine.containingNodes.Add(line.connectedNodes[0]);
                newHudLine.containingNodes.Add(line.connectedNodes[1]);
                newHudLine.ogLine = line;
                hudLines.Add(newHudLine);
            }
        }
    }

    IEnumerator ReCheckLines()
    {
        crIsRunning = true;
        yield return new WaitForSecondsRealtime(0.31f);

        if (myUserObj != null)
        {
            for (int i = hudLines.Count - 1; i >= 0; i--)
            {
                Destroy(hudLines[i].lineObj);
                hudLines.Remove(hudLines[i]);
            }

            List<Node> nodesOnBoard = new List<Node>();

            foreach (objOnBoard objOB in nodeObjsOnBoard)
            {
                nodesOnBoard.Add(objOB.associatedNode);
            }

            foreach (Line line in NodeManager.nM.lines)
            {
                //Is the line associated with a faction
                if (line.lineFaction == Faction.Neutral)
                {
                    continue;
                }

                //Is one of the nodes it on the HUD
                if (!nodesOnBoard.Contains(line.connectedNodes[0]))
                {
                    continue;
                }

                //Is the other node in it on the HUD (and also not the same node as node 1)
                if (!nodesOnBoard.Contains(line.connectedNodes[1]))
                {
                    continue;
                }

                bool breakOutForLoop = false;

                //Is this connection already represented by a line
                foreach (HUDLines hudLine in hudLines)
                {
                    if (hudLine.containingNodes.Contains(line.connectedNodes[0]) && hudLine.containingNodes.Contains(line.connectedNodes[1]))
                    {
                        breakOutForLoop = true;
                        continue;
                    }
                }

                if (breakOutForLoop)
                {
                    continue;
                }

                if (line.connectedNodes[0].userInformation.beliefs == line.connectedNodes[1].userInformation.beliefs)
                {
                    continue;
                }

                GameObject nodeObj1 = null;
                GameObject nodeObj2 = null;

                foreach (objOnBoard objOB in nodeObjsOnBoard)
                {
                    if (objOB.associatedNode == line.connectedNodes[0])
                    {
                        nodeObj1 = objOB.userObj;
                    }

                    if (objOB.associatedNode == line.connectedNodes[1])
                    {
                        nodeObj2 = objOB.userObj;
                    }
                }

                var newLine = Instantiate(HUDLineObj, HUDLineHolder.transform);
                newLine.transform.localPosition = Vector3.Lerp(nodeObj1.transform.localPosition, nodeObj2.transform.localPosition, 0.5f);
                newLine.transform.localPosition += Vector3.forward * 0.25f;

                if (nodeObj1.transform.localPosition.y == nodeObj2.transform.localPosition.y)
                {
                    newLine.transform.Rotate(0, 0, 90);
                }

                newLine.GetComponent<Image>().material = LevelManager.lM.GiveLineMaterial(line.lineFaction);
                var newHudLine = new HUDLines();
                newHudLine.lineObj = newLine;
                newHudLine.containingNodes = new List<Node>();
                newHudLine.containingNodes.Add(line.connectedNodes[0]);
                newHudLine.containingNodes.Add(line.connectedNodes[1]);
                newHudLine.ogLine = line;
                hudLines.Add(newHudLine);
            }
        }

        crIsRunning = false;
    }

    public void ClearAxes()
    {
        if (crIsRunning)
        {
            StopCoroutine("ReCheckLines");
            crIsRunning = false;
        }

        if (myUserObj != null)
        {
            Destroy(myUserObj);
        }

        if (connectedUserObjs.Count > 0)
        {
            for (int i = connectedUserObjs.Count - 1; i >= 0; i--)
            {

                Destroy(connectedUserObjs[i].userObj);
                connectedUserObjs.RemoveAt(i);
            }
        }

    }

    [SerializeField] private Node prevNode;

    [SerializeField] private GameObject userObj;
    [SerializeField] private Transform userObjsHolder;

    [SerializeField] private GameObject HUDLineObj;
    [SerializeField] private GameObject HUDLineHolder;

    List<objOnBoard> nodeObjsOnBoard = new List<objOnBoard>();

    [SerializeField] private GameObject myUserObj;
    [SerializeField] private List<objOnBoard> connectedUserObjs = new List<objOnBoard>();
    [SerializeField] private List<HUDLines> hudLines = new List<HUDLines>();

    [SerializeField] private AudioClip userReveal;
    [SerializeField] private AudioClip userMove;

    private bool crIsRunning = false;

    [System.Serializable]
    public struct objOnBoard
    { 
        public GameObject userObj;
        public Image userImage;
        public Image userFaction;
        public AudioSource userAudioSource;
        public Node associatedNode;
    }

    [System.Serializable]
    public struct HUDLines
    {
        public GameObject lineObj;
        public List<Node> containingNodes;
        public Line ogLine;
    }
}
