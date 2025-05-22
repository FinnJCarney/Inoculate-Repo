using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    }

    private void OnDestroy()
    {
        lM = null;
    }

    private void Update()
    {
        for (int i = 0; i < factionTimers.Count; i++)
        {
            factionTimer adjustedFactionTimer;
            adjustedFactionTimer.timer = factionTimers[i].timer;
            adjustedFactionTimer.faction = factionTimers[i].faction;

            float timeToReduceBy = Time.deltaTime;

            if (factionTimers[i].faction == Faction.UpRight)
            {
                timeToReduceBy *= (NodeManager.nM.redNodes.Count / (float)NodeManager.nM.nodes.Count);
            }

            if (factionTimers[i].faction == Faction.DownRight)
            {
                timeToReduceBy *= (NodeManager.nM.yellowNodes.Count / (float)NodeManager.nM.nodes.Count);
            }

            if (factionTimers[i].faction == Faction.DownLeft)
            {
                timeToReduceBy *= (NodeManager.nM.greenNodes.Count / (float)NodeManager.nM.nodes.Count);
            }

            if (factionTimers[i].faction == Faction.UpLeft)
            {
                timeToReduceBy *= (NodeManager.nM.blueNodes.Count / (float)NodeManager.nM.nodes.Count);
            }

            if (factionTimers[i].faction == Faction.Neutral)
            {
                timeToReduceBy *= (NodeManager.nM.neutralNodes.Count / (float)NodeManager.nM.nodes.Count);
            }

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

        foreach(levelFaction lvlFaction in levelFactions.Values)
        {
            if(lvlFaction.position == Vector2.zero)
            {
                continue;
            }

            if(Vector2.Distance(pos, lvlFaction.position) < 5f)
            {
                outputColor = Color.Lerp(lvlFaction.color, outputColor, Vector2.Distance(pos, lvlFaction.position) / 6f);
            }
        }

        return outputColor;
    }

    [SerializeField] public SerializableDictionary<Faction, levelFaction> levelFactions = new SerializableDictionary<Faction, levelFaction>();

    public Node playerNode;
    public Faction playerAllyFaction;

    [SerializeField] int numOfActionsPerTurn;

    [SerializeField] private float roundTimer;

    [SerializeField] private List<factionTimer> factionTimers = new List<factionTimer>();
    [SerializeField] private List<specificFactionSetting> specificFactionSettings = new List<specificFactionSetting>();



    public static LevelManager lM;

}

[System.Serializable]
public struct levelFaction
{
    public Color color;
    public Vector2 position;
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
