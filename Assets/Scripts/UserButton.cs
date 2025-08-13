using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserButton : MonoBehaviour
{
    [SerializeField] public Node relatedNode;
    [SerializeField] public ActionType type;
    [SerializeField] Image image;
    [SerializeField] GameObject text;

    public bool buttonEnabled;

    
    [SerializeField] Color enabledColor;
    [SerializeField] Color disabledColor;

    public void EnableButton(bool enable)
    {
        buttonEnabled = enable;
        text.SetActive(buttonEnabled);
        if (buttonEnabled)
        {
            image.color = enabledColor;    
        }
        else
        {
            image.color = disabledColor;
        }
    }


}
