using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    private void Start()
    {
        handleText.text = nodeHandle;
        ShowMenu(false);
    }

    public void ShowMenu(bool show)
    {
        menu.SetActive(show);
    }


    [SerializeField] GameObject menu;
    [SerializeField] string nodeHandle;
    [SerializeField] TextMeshPro handleText;
}
