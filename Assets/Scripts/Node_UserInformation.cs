using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_UserInformation : MonoBehaviour
{

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

