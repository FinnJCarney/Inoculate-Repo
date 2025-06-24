using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSection : MonoBehaviour
{
    public void SetLevelSection(Sprite image, string title, string description, int money)
    {
        levelImage.sprite = image;
        levelTitle.text = title;
        levelDesc.text = description;
        levelMoney.text = money.ToString();
    }

    public void ShowLevelContactScreen()
    {

    }

    [SerializeField] Image levelImage;
    [SerializeField] TextMeshProUGUI levelTitle;
    [SerializeField] TextMeshProUGUI levelDesc;
    [SerializeField] TextMeshProUGUI levelMoney;

}
