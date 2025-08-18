using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private void Awake()
    {
        if (lM != null)
        {
            Destroy(this);
        }
        else
        {
            lM = this;
        }

        AssembleValidSpaces();
        LayoutFactionGrid();
    }

    private void Start()
    {
        for (int i = 0; i < factionTimers.Count; i++)
        {
            factionTimer adjustedFactionTimer;
            adjustedFactionTimer.timer = factionTimers[i].timer;
            adjustedFactionTimer.faction = factionTimers[i].faction;

            adjustedFactionTimer.timer = roundTimer * (Random.Range(1f, 2f));
        }

        List<Faction> factionIndexes = new List<Faction>();
        factionIndexes.AddRange(levelFactions.Keys);

        for (int i = 0; i < levelFactions.Count; i++)
        {
            var adjustedLevelFaction = levelFactions[factionIndexes[i]];
            if (levelFactions[factionIndexes[i]].name == null)
            {
                adjustedLevelFaction.name = "<b>GIVE THIS FACTION A NAME</b>";
            }
            adjustedLevelFaction.positions = new List<Vector2>();
            adjustedLevelFaction.positions.Add(adjustedLevelFaction.mainPosition);
            adjustedLevelFaction.particleMaterial = new Material(defaultParticleMaterial);
            adjustedLevelFaction.particleMaterial.color = adjustedLevelFaction.color;
            adjustedLevelFaction.particleMaterial.SetColor("_EmissionColor", adjustedLevelFaction.color);
            levelFactions[factionIndexes[i]] = adjustedLevelFaction;
        }

        NodeManager.nM.AddNodeFactions();
        HUDManager.hM.SetMenuBounds(levelMap);
        InputManager.iM.SetMCC(mapCamera);
        LayerManager.lM.SetLayer(startingLayer, allowedLayers);
    }

    private void OnDestroy()
    {
        lM = null;
    }

    private void Update()
    {
        CheckFactionSpaces();
        UpdateFactionGrid();

        for (int i = 0; i < factionTimers.Count; i++)
        {
            if (!NodeManager.nM.nodeFactions.ContainsKey(factionTimers[i].faction))
            {
                Debug.LogWarning("Faction " + factionTimers[i].faction + " is not in NodeFactions");
                continue;
            }

            factionTimer adjustedFactionTimer;
            adjustedFactionTimer.timer = factionTimers[i].timer;
            adjustedFactionTimer.faction = factionTimers[i].faction;

            float timeToReduceBy = Time.deltaTime;

            timeToReduceBy *= NodeManager.nM.nodeFactions[adjustedFactionTimer.faction].Count / (float)NodeManager.nM.nodes.Count;

            timeToReduceBy = Mathf.Clamp(timeToReduceBy, Time.deltaTime * 0.4f, Time.deltaTime * 0.8f);

            adjustedFactionTimer.timer -= timeToReduceBy;

            if (adjustedFactionTimer.timer < 0f)
            {
                ActionManager.aM.PerformAIAction(numOfActionsPerTurn, factionTimers[i].faction);

                bool specificTimerFound = false;

                foreach (var specificFactionSetting in specificFactionSettings)
                {
                    if (specificFactionSetting.faction == adjustedFactionTimer.faction)
                    {
                        adjustedFactionTimer.timer = specificFactionSetting.specificTimer * (Random.Range(0.75f, 1.25f));
                        specificTimerFound = true;
                    }
                }

                if (!specificTimerFound)
                {
                    adjustedFactionTimer.timer = roundTimer * (Random.Range(0.75f, 1.25f));
                }
            }

            factionTimers[i] = adjustedFactionTimer;
        }
    }

    public Color GiveAverageColor(Vector2 pos)
    {
        Color outputColor = Color.white;

        foreach (levelFaction lvlFaction in levelFactions.Values)
        {
            if(lvlFaction.mainPosition == Vector2.zero)
            {
                continue;
            }

            outputColor = Color.Lerp(lvlFaction.color, outputColor, (Vector2.Distance(pos, lvlFaction.mainPosition) / 5f));
        }

        return outputColor;
    }

    public Material GiveLineMaterial(Faction faction)
    {
        return levelFactions[faction].lineMaterial;
    }

    private void AssembleValidSpaces()
    {
        List<GridMarker> gridMarkers = new List<GridMarker>();
        gridMarkers.AddRange(GetComponentsInChildren<GridMarker>());

        for (int i = gridMarkers.Count - 1; i >= 0; i--)
        {
            GridMarker gridMarker = gridMarkers[i];
            Vector2 gridPos = new Vector2(Mathf.Round(gridMarker.transform.position.x), Mathf.Round(gridMarker.transform.position.z));

            nodeGroups.Add(gridPos, Instantiate<GameObject>(NodeGroupObj, new Vector3(gridPos.x, 0, gridPos.y), Quaternion.identity).GetComponent<NodeGroup>());
            nodeGroups[gridPos].name = ("Node Group: " + gridPos);
            nodeGroups[gridPos].nodesInGroup = new List<Node_UserInformation>();

            Destroy(gridMarker.gameObject);
        }
    }

    private void LayoutFactionGrid()
    {
        foreach(Vector2 validSpace in nodeGroups.Keys)
        {
            for (int i = 0; i < 225; i++)
            {
                float zVal = validSpace.y + (((Mathf.Floor(i / 15f) - 7f)) * 3f);
                float xVal = validSpace.x + ((i - ((Mathf.Floor(i / 15f) * 15f) + 7f)) * 3f);
                Vector2 possiblePosition = new Vector2(xVal, zVal);

                if (Vector2.Distance(possiblePosition, validSpace) > 24f)
                {
                    continue;
                }

                if (!factionGridMarkers.ContainsKey(possiblePosition))
                {
                    var newFacGridMarker = Instantiate<GameObject>(factionGridMarker, new Vector3(possiblePosition.x, -1f, possiblePosition.y), Quaternion.Euler(90f, 0f, 0f), this.transform);
                    factionGridMarkers.Add(possiblePosition, newFacGridMarker.GetComponent<FactionGridMarker>());
                    factionGridMarkers[possiblePosition].SetFaction(Faction.Neutral);
                }
            }
        }

    }

    public void UpdateFactionGrid()
    {
        foreach(Vector2 factionGridMarkerPos in factionGridMarkers.Keys)
        {
            List<Faction> possibleAlliedFaction = new List<Faction>();

            foreach(Faction faction in levelFactions.Keys)
            {
                if(faction == Faction.Neutral)
                {
                    continue;
                }

                foreach(Vector2 factionPositions in levelFactions[faction].positions)
                {
                    if(possibleAlliedFaction.Contains(faction))
                    {
                        continue;
                    }

                    if(Vector2.Distance(factionPositions, factionGridMarkerPos) < 13.5f)
                    {
                        possibleAlliedFaction.Add(faction);
                    }
                    //Use distance to control speed?
                }
            }

            if(possibleAlliedFaction.Count == 1)
            {
                factionGridMarkers[factionGridMarkerPos].SetFaction(possibleAlliedFaction[0]);
            }

            else if(possibleAlliedFaction.Count == 0)
            {
                factionGridMarkers[factionGridMarkerPos].SetFaction(Faction.Neutral);
            }

            else if (possibleAlliedFaction.Count > 1)
            {
                factionGridMarkers[factionGridMarkerPos].SetFaction(Faction.Clashing);
            }
        }
    }

    public bool CheckValidSpace(Vector2 spotToCheck)
    {
        return nodeGroups.Contains(spotToCheck);
    }

    public void CheckFactionSpaces()
    {
        foreach(Faction faction in levelFactions.Keys)
        {
            if(faction == Faction.Neutral)
            {
                continue;
            }

            for(int i = levelFactions[faction].positions.Count - 1; i >= 0; i--)
            {
                bool hasCorrespondingAllliedNode = false;

                if (levelFactions[faction].positions[i] == levelFactions[faction].mainPosition)
                {
                    hasCorrespondingAllliedNode = true;
                }

                foreach(NodeGroup nodeGroup in LevelManager.lM.nodeGroups.Values)
                {
                    if(nodeGroup.nodesInGroup.Count == 0)
                    {
                        nodeGroup.groupFaction = Faction.Neutral;
                        continue;
                    }

                    if (levelFactions[faction].positions[i] == nodeGroup.groupBelief && CheckIfFactionSpaceConnectedToInstigator(faction, levelFactions[faction].positions[i]))
                    {
                        hasCorrespondingAllliedNode = true;
                    }
                }

                if(hasCorrespondingAllliedNode == false)
                {
                    levelFactions[faction].positions.RemoveAt(i);
                }
            }
        }
    }

    public bool CheckIfFactionSpaceConnectedToInstigator(Faction faction, Vector2 position)
    {
        List<Vector2> positionsToCheck = new List<Vector2>();

        List<Vector2> positionsChecked = new List<Vector2>();

        positionsToCheck.Add(position);

        Vector2 movement = new Vector2(12f, 12f);

        for (int i = 0; i < positionsToCheck.Count; i++)
        {
            if (positionsToCheck[i] == levelFactions[faction].mainPosition)
            {
                return true;
            }

            if(positionsChecked.Contains(positionsToCheck[i]))
            {
                continue;
            }

            if (levelFactions[faction].positions.Contains(positionsToCheck[i] + (movement * Vector2.up)))
            {
                positionsToCheck.Add(positionsToCheck[i] + (movement * Vector2.up));
            }

            if (levelFactions[faction].positions.Contains(positionsToCheck[i] + (movement * Vector2.right)))
            {
                positionsToCheck.Add(positionsToCheck[i] + (movement * Vector2.right));
            }

            if (levelFactions[faction].positions.Contains(positionsToCheck[i] + (movement * Vector2.left)))
            {
                positionsToCheck.Add(positionsToCheck[i] + (movement * Vector2.left));
            }

            if (levelFactions[faction].positions.Contains(positionsToCheck[i] + (movement * Vector2.down)))
            {
                positionsToCheck.Add(positionsToCheck[i] + (movement * Vector2.down));
            }

            positionsChecked.Add(positionsToCheck[i]);
        }

        return false;
    }

    [SerializeField] public LevelInfo levelInfo;

    public Node playerNode;
    public Faction playerAllyFaction;

    public GameMode gameMode;
    public float amountRequiredForControl;

    public SerializableDictionary<Faction, levelFaction> levelFactions = new SerializableDictionary<Faction, levelFaction>();
    public SerializableDictionary<ActionType, TweetInfo> tweetsForActions = new SerializableDictionary<ActionType, TweetInfo>();

    [TextArea(5, 5)]
    public string levelMap = "11111\n11111\n11111\n11111\n11111";

    [SerializeField] public connectionLayer allowedLayers;
    [SerializeField] connectionLayer startingLayer;

    [SerializeField] int numOfActionsPerTurn;

    [SerializeField] private float roundTimer;

    [SerializeField] private List<factionTimer> factionTimers = new List<factionTimer>();
    [SerializeField] private List<specificFactionSetting> specificFactionSettings = new List<specificFactionSetting>();

    [SerializeField] public string currentScene;

    [SerializeField] public MapCameraController mapCamera;

    [SerializeField] private Material defaultParticleMaterial;

    [SerializeField] private GameObject NodeGroupObj;
    [SerializeField] private GameObject factionGridMarker;

    public SerializableDictionary<Vector2, NodeGroup> nodeGroups = new SerializableDictionary<Vector2, NodeGroup>();
    private Dictionary<Vector2, FactionGridMarker> factionGridMarkers = new Dictionary<Vector2, FactionGridMarker>();

    public static LevelManager lM;

}

[System.Serializable]
public struct levelFaction
{
    public string name;
    public Color color;
    public Vector2 mainPosition;
    public List<Vector2> positions;
    public Material lineMaterial;
    public Material particleMaterial;
    public GameObject actionLine;
}


[System.Serializable]
public struct factionTimer
{
    public float timer;
    public Faction faction;
}

[System.Serializable]
public struct specificFactionSetting
{
    public Faction faction;
    public float specificTimer;
    public float specificNumOfTurns;
}

public enum GameMode
{ 
    MisinformerHunt,
    SpecificNodeCapture,
    MapControl,
    FactionElimination,
    None
}

