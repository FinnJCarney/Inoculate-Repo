using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[ExecuteInEditMode]

public class Node_UserInformation : MonoBehaviour
{
    private void Update()
    {
        if(syncInfo)
        {
            if(NodeName == "")
            {
                NodeName = this.name;
            }

            foreach(Node connectedNode in connectedNodes.Keys)
            {
                if (connectedNode.userInformation.connectedNodes.ContainsKey(this.GetComponent<Node>()))
                {
                    if (connectedNode.userInformation.connectedNodes[this.GetComponent<Node>()].layer != connectedNodes[connectedNode].layer)
                    {
                        connectedNode.userInformation.connectedNodes.Remove(this.GetComponent<Node>());
                    }

                    if (connectedNode.userInformation.connectedNodes[this.GetComponent<Node>()].type != connectedNodes[connectedNode].type)
                    {
                        if (connectedNode.userInformation.connectedNodes[this.GetComponent<Node>()].type == connectionType.influencedBy && connectedNodes[connectedNode].type == connectionType.influenceOn || connectedNode.userInformation.connectedNodes[this.GetComponent<Node>()].type == connectionType.influenceOn && connectedNodes[connectedNode].type == connectionType.influencedBy)
                        {
                            continue;
                        }

                        connectedNode.userInformation.connectedNodes.Remove(this.GetComponent<Node>());
                    }
                }

                if (!connectedNode.userInformation.connectedNodes.ContainsKey(this.GetComponent<Node>()))
                {
                    connectedNodeInfo newConnectedNodeInfo = new connectedNodeInfo();
                    newConnectedNodeInfo.layer = connectedNodes[connectedNode].layer;
                    newConnectedNodeInfo.type = connectionType.mutual;

                    if (connectedNodes[connectedNode].type == connectionType.influenceOn)
                    {
                        newConnectedNodeInfo.type = connectionType.influencedBy;
                    }
                    else if (connectedNodes[connectedNode].type == connectionType.influencedBy)
                    {
                        newConnectedNodeInfo.type = connectionType.influenceOn;
                    }

                    connectedNode.userInformation.connectedNodes.Add(this.GetComponent<Node>(), newConnectedNodeInfo);
                }
            }

            syncInfo = false;
        }
    }

    [SerializeField] bool syncInfo = false;

    [Header("Basic Information")]
    public string NodeName;

    public Vector2 beliefs;
    public bool userInfoHidden;

    public bool isPlayer;
    public Faction faction;
    public Faction instigator;

    public levelFaction levelFaction;

    public bool misinformerHori;
    public bool misinformerVert;

    public bool toCapture;

    public SerializableDictionary<Node, connectedNodeInfo> connectedNodes = new SerializableDictionary<Node, connectedNodeInfo>();
}

public enum Faction
{ 
    UpRight,
    UpLeft,
    DownLeft,
    DownRight,
    Neutral,
    UpCenter,
    DownCenter,
    CenterRight,
    CenterLeft,
    None
}

[System.Serializable]
public struct connectedNodeInfo
{
    public connectionLayer layer;
    public connectionType type;
}

public enum connectionLayer
{
    onlineOffline,
    online,
    offline
}

public enum connectionType
{
    mutual,
    influencedBy,
    influenceOn
}

