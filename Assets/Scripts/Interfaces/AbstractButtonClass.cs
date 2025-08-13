using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractButtonClass : MonoBehaviour
{

    [SerializeField] private Image buttonBack;
    [SerializeField] private Image buttonIcon;

    [SerializeField] private Color unhoveredColor;
    [SerializeField] private Color hoverColor;

    public abstract void PerformAction();

    public void OnHover(bool hovered)
    {
        if (buttonBack == null)
        {
            return;
        }

        if (hovered)
        {
            buttonBack.color = hoverColor;
        }
        else
        {
            buttonBack.color = unhoveredColor;
        }
    }
}

