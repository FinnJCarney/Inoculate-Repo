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

            foreach (GameObject actionObject in adjustedLevelFaction.actionObjects)
            {
                adjustedLevelFaction.availableActions.Add(actionObject.GetComponent<Action>());
            }
            
            levelFactions[factionIndexes[i]] = adjustedLevelFaction;

        }

        NodeManager.nM.AddNodeFactions();
        //HUDManager.hM.SetMenuBounds(levelMap);
        InputManager.iM.SetMCC(mapCamera);
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

            //timeToReduceBy *= NodeManager.nM.nodeFactions[adjustedFactionTimer.faction].Count / (float)NodeManager.nM.nodes.Count;
            
            //timeToReduceBy = Mathf.Clamp(timeToReduceBy, Time.deltaTime * 0.4f, Time.deltaTime * 0.8f);

            adjustedFactionTimer.timer -= timeToReduceBy;

            if (adjustedFactionTimer.timer < 0f)
            {
                ActionManager.aM.PerformAIAction(numOfActionsPerTurn, factionTimers[i].faction);

                bool specificTimerFound = false;

                foreach (var specificFactionSetting in specificFactionSettings)
                {
                    if (specificFactionSetting.faction == adjustedFactionTimer.faction)
                    {
                        adjustedFactionTimer.timer = specificFactionSetting.specificTimer;
                        specificTimerFound = true;
                    }
                }

                if (!specificTimerFound)
                {
                    adjustedFactionTimer.timer = roundTimer;
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

    public float MinDistanceBetweenTwoVector2sOnMap(Vector2 startingPos, Vector2 endingPos) //Move to LevelManager
    {
        Vector2 movement = new Vector2(12f, 12f);
        List<Vector2> positionsToCheck = new List<Vector2>();
        Dictionary<Vector2, List<Vector2>> possiblePaths = new Dictionary<Vector2, List<Vector2>>();

        positionsToCheck.Add(startingPos);

        List<Vector2> startingPath = new List<Vector2>();
        startingPath.Add(startingPos);
        possiblePaths.Add(startingPos, startingPath);

        //Debug.Log("Checking for path from " + startingPos + " to " + endingPos);

        for (int i = 0; i < positionsToCheck.Count; i++) //Check list of Vector2s that are valid
        {
            Vector2 positionToCheck = positionsToCheck[i];

            List<Vector2> positionsToAdd = new List<Vector2>();

            //Check all adjacent Vector2, if they are available, we should look at them
            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.up * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.up * movement));
            }

            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.down * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.down * movement));
            }

            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.right * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.right * movement));
            }

            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.left * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.left * movement));
            }

            foreach (Vector2 positionToAdd in positionsToAdd)
            {
                //We're gonna see what a path to this new Vector2 looks like
                List<Vector2> newPossiblePath = new List<Vector2>();

                //Have we been to our Vector2 before? If so, grab the current path there now
                if (possiblePaths.ContainsKey(positionToCheck))
                {
                    //Debug.Log("Possible paths contains " + positionToCheck);
                    foreach (Vector2 pathPos in possiblePaths[positionToCheck])
                    {
                        newPossiblePath.Add(pathPos);
                    }
                    newPossiblePath.Add(positionToAdd);
                }
                else //Else, we have no record of how to get here, which probably means it's very close
                {
                    //Debug.Log("Possible paths does not contain " + positionToCheck);
                    newPossiblePath.Add(positionToCheck);
                    newPossiblePath.Add(positionToAdd);
                }

                //We have established a path to this new vector2 we're checking
                //Debug.Log("Possible Path between " + newPossiblePath[0] + " and " + positionToAdd + " is " + newPossiblePath.Count);

                //We've never been to this Vector2 before! Whatever path we've developed to it is currently our best one, so let's add that path and check what's around it in future
                if (!possiblePaths.ContainsKey(positionToAdd))
                {
                    //Debug.Log("Possible paths does not contain " + positionToAdd);
                    possiblePaths.Add(positionToAdd, newPossiblePath);
                    positionsToCheck.Add(positionToAdd);
                }
                else //We have been to this vector 2 before, so check how long it previously took to get here vs our new path. Save whichever one is shorter
                {
                    //Debug.Log("Possible paths does contain " + positionToAdd + "with length " + possiblePaths[positionToAdd].Count);
                    if (newPossiblePath.Count < possiblePaths[positionToAdd].Count)
                    {
                        //Debug.Log(newPossiblePath.Count + " is smaller than " + possiblePaths[positionToAdd].Count);
                        possiblePaths[positionToAdd].Clear();

                        foreach (Vector2 pathPos in newPossiblePath)
                        {
                            possiblePaths[positionToAdd].Add(pathPos);
                        }

                        positionsToCheck.Add(positionToAdd);
                    }
                }

                //Debug.Log("New smallest path between " + startingPos + " and " + positionToAdd + " is " + possiblePaths[positionToAdd].Count);
            }
        }

        //At this stage, we should have checked essentially every point on the board, and in that process found the shortest path to our ideal end point, so return that
        //Debug.Log("Shortest Path between " + startingPos + " and " + endingPos + " is " + possiblePaths[endingPos].Count);

        return possiblePaths[endingPos].Count;
    }

    [SerializeField] public LevelInfo levelInfo;

    public Node playerNode;
    public Faction playerAllyFaction;

    public GameMode gameMode;
    public float amountRequiredForControl;

    public SerializableDictionary<Faction, levelFaction> levelFactions = new SerializableDictionary<Faction, levelFaction>();
    public SerializableDictionary<Action, TweetInfo> tweetsForActions = new SerializableDictionary<Action, TweetInfo>();

    [SerializeField] int numOfActionsPerTurn;

    [SerializeField] public float roundTimer;

    [SerializeField] public List<factionTimer> factionTimers = new List<factionTimer>();
    [SerializeField] public List<specificFactionSetting> specificFactionSettings = new List<specificFactionSetting>();

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
    [HideInInspector] public List<Action> availableActions;
    public GameObject[] actionObjects; 
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

