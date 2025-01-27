using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleScreenManager : MonoBehaviour
{

    void Update()
    {
        Title.color = Color.Lerp(Color.clear, Color.white, (Time.time / 3) - titleAppear);
        SubTitle.color = Color.Lerp(Color.clear, Color.white, (Time.time / 2) - subtitleAppear);

        if ((Time.time / 2) - instructionsAppear > 0)
        {
            Instructions.color = Color.Lerp(Color.clear, Color.white, Mathf.Abs(Mathf.Sin(Time.time)));
        }
        else
        {
            Instructions.color = Color.clear;
        }
    }

    [SerializeField] TextMeshPro Title;
    [SerializeField] float titleAppear;

    [SerializeField] TextMeshPro SubTitle;
    [SerializeField] float subtitleAppear;

    [SerializeField] TextMeshPro Instructions;
    [SerializeField] float instructionsAppear;
}
