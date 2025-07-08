using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSection : MonoBehaviour
{
    public void SetLevelSection(LevelInfo newLevelInfo, Sprite image, string title, string description, int money)
    {
        levelInfo = newLevelInfo;
        levelImage.sprite = image;
        levelTitle.text = title;
        levelDesc.text = description;
        levelMoney.text = "£" + money.ToString();
    }

    public void ShowLevelContactScreen()
    {
        LevelSelectManager.lsm.SwitchToChatPage(levelInfo);
    }

    [SerializeField] LevelInfo levelInfo;
    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelTitle;
    [SerializeField] TextMeshProUGUI levelDesc;
    [SerializeField] TextMeshProUGUI levelMoney;

}
