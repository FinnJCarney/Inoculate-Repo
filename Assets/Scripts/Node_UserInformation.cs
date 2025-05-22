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

    [SerializeField] public List<Node> connectedNodes = new List<Node>();
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

