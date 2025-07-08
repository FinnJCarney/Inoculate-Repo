using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPage : MonoBehaviour
{
    public void Initialize(LevelInfo newLevelInfo)
    {
        levelInfo = newLevelInfo;
        clientName.text = newLevelInfo.LevelContact;
        clientImage.sprite = newLevelInfo.LevelImage;

        AdvanceChat();
    }

    private void Update()
    {
        if (active && chatMessages.Count > 0 && curTextNum != levelInfo.chatMessages.Length)
        {
            timer += Time.deltaTime;

            if(nextTime < timer)
            {
                if(chatMessages[curTextNum].chatText.text != levelInfo.chatMessages[curTextNum].Text)
                {
                    RevealChat();
                }
                else
                {
                    curTextNum++;
                    AdvanceChat();
                }
            }
        }

        contentHolder.GetComponent<VerticalLayoutGroup>().spacing = 0;
        contentHolder.GetComponent<VerticalLayoutGroup>().padding.top = 0;
        contentHolder.GetComponent<VerticalLayoutGroup>().spacing = 16f;
        contentHolder.GetComponent<VerticalLayoutGroup>().padding.top = 48;
    }

    private void AdvanceChat()
    {
        if (curTextNum < levelInfo.chatMessages.Length)
        {
            var newChatMessage = Instantiate<GameObject>(chatMessageObj, contentHolder);
            newChatMessage.GetComponent<HorizontalLayoutGroup>().padding.left = levelInfo.chatMessages[curTextNum].clientText ? -480 : 480;
            chatMessages.Add(newChatMessage.GetComponent<ChatMessage>());

            if (levelInfo.chatMessages[curTextNum].clientText) { chatMessages[curTextNum].chatBackground.color = levelInfo.clientColor; }

            nextTime = timer + Mathf.Clamp(0.05f * levelInfo.chatMessages[curTextNum].Text.Length, 0.25f, 4f);
        }
    }

    private void RevealChat()
    {
        chatMessages[curTextNum].chatText.text = levelInfo.chatMessages[curTextNum].Text;
        nextTime = timer += 0.35f;
    }

    public void BackToLevelSelect()
    {
        LevelSelectManager.lsm.SwitchToLevelSelect();
    }

    public void PlayLevel()
    {
        StateManager.sM.LoadLevelFromLevelSelect(levelInfo);
    }

    public bool active;

    private float timer;
    private float nextTime = 1000f;

    LevelInfo levelInfo;
    private int curTextNum = 0;

    [SerializeField] TextMeshProUGUI clientName;
    [SerializeField] Image clientImage;

    [SerializeField] private GameObject chatMessageObj;
    public List<ChatMessage> chatMessages = new List<ChatMessage>();

    [SerializeField] public CustomScrollView cSV;

    [SerializeField] Transform contentHolder;
}
