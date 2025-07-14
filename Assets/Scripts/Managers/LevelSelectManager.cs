using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    private void Awake()
    {
    
        lsm = this;
    
    }
    
    private void OnDestroy()
    {
        if (lsm = this)
        {
            lsm = null;
        }
    }

    private void Start()
    {
        LoadLevels();
    }

    public void LoadLevels()
    {
        foreach (LevelInfo levelInfo in StateManager.sM.LevelsToDisplay)
        {
            LevelSection newLevelSection = Instantiate<GameObject>(levelSectionPrefab, levelSorter.transform).GetComponent<LevelSection>();
            newLevelSection.SetLevelSection(levelInfo, levelInfo.LevelImage, levelInfo.LevelContact, levelInfo.LevelDescription, levelInfo.LevelMoney);
        }

        levelSelectScrollView.ResetHandle();
    }

    public void SwitchToChatPage(LevelInfo levelInfo)
    {
        SelectPage.transform.DOLocalMoveX(-1500, 0.5f);
        if(!chatPages.ContainsKey(levelInfo))
        {
            var newChatPage = Instantiate<GameObject>(chatPage, this.transform);
            newChatPage.transform.localPosition = new Vector3(1500, 0, 0);
            newChatPage.transform.DOLocalMoveX(0, 0.5f);
            newChatPage.GetComponent<ChatPage>().Initialize(levelInfo);
            chatPages.Add(levelInfo, newChatPage.GetComponent<ChatPage>());
            chatPages[levelInfo].active = true;
        }
        else
        {
            chatPages[levelInfo].transform.DOLocalMoveX(0, 0.5f);
            chatPages[levelInfo].active = true;
        }
    }

    public void SwitchToLevelSelect()
    {
        SelectPage.transform.DOLocalMoveX(0, 0.5f);
        foreach(ChatPage chatPage in chatPages.Values)
        {
            chatPage.transform.DOLocalMoveX(1500, 0.5f);
            chatPage.active = false;
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

    public static LevelSelectManager lsm;

    [SerializeField] private GameObject SelectPage;

    List<LevelInfo> levelsToLoad = new List<LevelInfo>();
    [SerializeField] private CustomScrollView levelSelectScrollView;

    [SerializeField] private GameObject levelSectionPrefab;
    [SerializeField] Transform levelSorter;

    [SerializeField] private GameObject chatPage;
    
    Dictionary<LevelInfo, ChatPage> chatPages = new Dictionary<LevelInfo, ChatPage>();
}
