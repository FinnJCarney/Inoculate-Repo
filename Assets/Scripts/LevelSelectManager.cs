using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    private void Start()
    {
        LoadLevels();
    }

    public void LoadLevels()
    {
        foreach (LevelInfo levelInfo in StateManager.sM.LevelsToDisplay)
        {
            LevelSection newLevelSection = Instantiate<GameObject>(levelSectionPrefab, levelSorter.transform).GetComponent<LevelSection>();
            newLevelSection.SetLevelSection(levelInfo.LevelImage, levelInfo.LevelContact, levelInfo.LevelDescription, levelInfo.LevelMoney);

        }
    }

    public void AddLevelsToLoad(LevelInfo newLevel)
    {
        if(!levelsToLoad.Contains(newLevel))
        {
            levelsToLoad.Add(newLevel);
        }
    }

    public void RemoveLevelsToLoad(LevelInfo newLevel)
    {
        if (levelsToLoad.Contains(newLevel))
        {
            levelsToLoad.Remove(newLevel);
        }
    }

    List<LevelInfo> levelsToLoad = new List<LevelInfo>();

    [SerializeField] private GameObject levelSectionPrefab;
    [SerializeField] Transform levelSorter;
}
