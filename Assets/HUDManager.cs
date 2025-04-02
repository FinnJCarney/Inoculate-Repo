using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    private void Awake()
    {
        if(i != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            i = this;
        }
    }

    private void Start()
    {
        var canvas = GetComponent<Canvas>();

        canvas.worldCamera = Camera.main;

        foreach(UserButton userButton in userButtons)
        {
            var UIButton = userButton.GetComponent<Button>();
            UIButton.onClick.AddListener(delegate { ActionManager.aM.PerformButtonAction(userButton); });
        }

        SyncMenu(null);
    }

    private void Update()
    {
        if(selectedNode != null)
        {
            SyncNodeOptions();
        }
    }

    private void SyncNodeOptions()
    {
        bannedCover.SetActive(selectedNode.isBanned);
        hiddenCover.SetActive(selectedNode.userInformation.userInfoHidden);



        int theorheticalActions = 0;
        int possibleActions = 0;

        bool hasLeftNeighbourAvail = false;
        bool hasRightNeighbourAvail = false;
        bool hasUpNeighbourAvail = false;
        bool hasDownNeighbourAvail = false;
        bool hasAllyNeighbourAvail = false;

        foreach (Node connectedNode in selectedNode.connectedNodes)
        {
            if (connectedNode.isBanned || !connectedNode.connectedNodes.Contains(selectedNode))
            {
                continue;
            }

            if (connectedNode.performingAction)
            {
                theorheticalActions++;
                continue;
            }

            if (connectedNode.userInformation.allyStatus == LevelManager.lM.playerAllyFaction)
            {
                theorheticalActions++;
                possibleActions++;
                hasAllyNeighbourAvail = true;

                if (connectedNode.userInformation.beliefs.x == selectedNode.userInformation.beliefs.x)
                {
                    hasLeftNeighbourAvail = true;
                    hasRightNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.x > selectedNode.userInformation.beliefs.x)
                {
                    hasRightNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.x < selectedNode.userInformation.beliefs.x)
                {
                    hasLeftNeighbourAvail = true;
                }

                if (connectedNode.userInformation.beliefs.y == selectedNode.userInformation.beliefs.y)
                {
                    hasUpNeighbourAvail = true;
                    hasDownNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.y > selectedNode.userInformation.beliefs.y)
                {
                    hasUpNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.y < selectedNode.userInformation.beliefs.y)
                {
                    hasDownNeighbourAvail = true;
                }
            }
        }
    
        bool connectActionAvailable = false;

        if (!selectedNode.userInformation.userInfoHidden && NodeManager.nM.centristNodes.Count > 0)
        {
            foreach (Node centristNode in NodeManager.nM.centristNodes)
            {
                if (!selectedNode.connectedNodes.Contains(centristNode) && centristNode != this)
                {
                    connectActionAvailable = true;
                }
            }
        }

        if (!selectedNode.isBanned)
        {
            but_DM.EnableButton(selectedNode.userInformation.userInfoHidden && hasAllyNeighbourAvail);
            but_Accuse.EnableButton(!selectedNode.userInformation.userInfoHidden && hasAllyNeighbourAvail);
            but_Left.EnableButton(hasLeftNeighbourAvail && !selectedNode.userInformation.userInfoHidden);
            but_Right.EnableButton(hasRightNeighbourAvail && !selectedNode.userInformation.userInfoHidden);
            but_Up.EnableButton(hasUpNeighbourAvail && !selectedNode.userInformation.userInfoHidden);
            but_Down.EnableButton(hasDownNeighbourAvail && !selectedNode.userInformation.userInfoHidden);
            but_Connect.EnableButton(connectActionAvailable);
        }
        else
        {
            foreach(UserButton userButton in userButtons)
            {
                userButton.EnableButton(false);
            }
        }
    }

    public void SyncMenu(Node node)
    {
        if (node == null)
        {
            userName.text = "...";

            foreach (UserButton userButton in userButtons)
            {
                userButton.relatedNode = null;
            }

            but_DM.EnableButton(false);
            but_Accuse.EnableButton(false);
            but_Left.EnableButton(false);
            but_Right.EnableButton(false);
            but_Up.EnableButton(false);
            but_Down.EnableButton(false);
            but_Connect.EnableButton(false);

            politicalAxes.ClearAxes();

            selectedNode = null;

            return;
        }

        userName.text = node.userInformation.name;

        foreach(UserButton userButton in userButtons)
        {
            userButton.relatedNode = node;
        }

        selectedNode = node;

        politicalAxes.SyncPoliticalAxes(selectedNode, true);
    }

    public void SyncPoliticalAxes()
    {
        if(selectedNode != null)
        {
            politicalAxes.SyncPoliticalAxes(selectedNode, false);
        }
    }

    public static HUDManager i;

    [SerializeField] private Node selectedNode;

    [SerializeField] public Node_PoliticalAxes politicalAxes;
    [SerializeField] private GameObject hiddenCover;
    [SerializeField] private GameObject bannedCover;

    [SerializeField] private TextMeshProUGUI userName;
    [SerializeField] private List<UserButton> userButtons = new List<UserButton>();

    [SerializeField] public UserButton but_DM;
    [SerializeField] public UserButton but_Accuse;
    [SerializeField] public UserButton but_Left;
    [SerializeField] public UserButton but_Right;
    [SerializeField] public UserButton but_Up;
    [SerializeField] public UserButton but_Down;
    [SerializeField] public UserButton but_Connect;
}
