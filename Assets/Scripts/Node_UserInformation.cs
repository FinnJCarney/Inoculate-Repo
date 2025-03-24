using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_UserInformation : MonoBehaviour
{

    [Header("Basic Information")]
    public string NodeName;

    public Vector2 beliefs;
    public bool userInfoHidden;

    public bool misinformerHori;
    public bool misinformerVert;

    [SerializeField] public List<Node> connectedNodes = new List<Node>();
}
