using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]

public class Node_UserInformation : MonoBehaviour
{
    private void Awake()
    {
        nodeCore = GetComponent<Node>();
        beliefs = new Vector2(this.transform.position.x, this.transform.position.z);
    }

    private void Update()
    {
        if(syncInfo)
        {
            if(NodeName == "")
            {
                NodeName = this.name;
            }
            else
            {
                gameObject.name = NodeName;
            }

            nameTMP.text = NodeName;

            foreach(Node_UserInformation connectedNode in connectedNodes.Keys)
            {
                if (connectedNode.connectedNodes.ContainsKey(this))
                {
                    if (connectedNode.connectedNodes[this].type != connectedNodes[connectedNode].type)
                    {
                        if (connectedNode.connectedNodes[this].type == connectionType.influencedBy && connectedNodes[connectedNode].type == connectionType.influenceOn || connectedNode.connectedNodes[this].type == connectionType.influenceOn && connectedNodes[connectedNode].type == connectionType.influencedBy)
                        {
                            continue;
                        }

                        connectedNode.connectedNodes.Remove(this.GetComponent<Node>());
                    }
                }

                if (!connectedNode.connectedNodes.ContainsKey(this))
                {
                    connectedNodeInfo newConnectedNodeInfo = new connectedNodeInfo();
                    newConnectedNodeInfo.type = connectionType.mutual;

                    if (connectedNodes[connectedNode].type == connectionType.influenceOn)
                    {
                        newConnectedNodeInfo.type = connectionType.influencedBy;
                    }
                    else if (connectedNodes[connectedNode].type == connectionType.influencedBy)
                    {
                        newConnectedNodeInfo.type = connectionType.influenceOn;
                    }

                    connectedNode.connectedNodes.Add(this, newConnectedNodeInfo);
                }
            }

            syncInfo = false;
        }
    }

    [SerializeField] bool syncInfo = false;

    [Header("Basic Information")]
    public string NodeName;
    public Sprite NodeImage;

    public Vector2 beliefs;
    public bool userInfoHidden;

    public bool isPlayer;
    public bool isBanned;
    public Faction faction;
    public Faction instigator;

    public levelFaction levelFaction;

    public bool misinformerHori;
    public bool misinformerVert;

    public bool toCapture;

    [SerializeField] private TextMeshPro nameTMP;

    public SerializableDictionary<Node_UserInformation, connectedNodeInfo> connectedNodes = new SerializableDictionary<Node_UserInformation, connectedNodeInfo>();

    private Tween movementTween;

    public Node nodeCore;
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
    Clashing,
    None
}

[System.Serializable]
public struct connectedNodeInfo
{
    public connectionType type;
}

public enum connectionType
{
    mutual,
    influencedBy,
    influenceOn
}

public enum Personality_NO
{
    Any,
    Intuitive,
    Observant
}

public enum Personality_TF
{
    Any,
    Thinking,
    Feeling
}

public enum Personality_Special
{ 
    None,
    Special1,
    Special2,
    Special3,
    Special4
}



