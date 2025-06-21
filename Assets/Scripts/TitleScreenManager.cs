using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TitleScreenManager : MonoBehaviour
{

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            clickNumber += 1;
        }

        Title.color = Color.Lerp(Color.clear, Color.white, (Time.time * 2) - titleAppear);
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

    private void FixedUpdate()
    {
        if (clickNumber < movePos.Length)
        {
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, movePos[clickNumber], 0.25f);
        }
        else
        {
            if (!begunLoad)
            {
                SceneLoader.sL.LoadManagers();
                SceneLoader.sL.LoadHUD();
                SceneLoader.sL.LoadSceneAdditive("Level1");
                begunLoad = true;
            }
            SceneLoader.sL.UnloadScene("TitleScreen");
        }
    }

    [SerializeField] TextMeshPro Title;
    [SerializeField] float titleAppear;

    [SerializeField] TextMeshPro SubTitle;
    [SerializeField] float subtitleAppear;

    [SerializeField] TextMeshPro Instructions;
    [SerializeField] float instructionsAppear;

    int clickNumber;
    [SerializeField] Vector3[] movePos;
    [SerializeField] string Level1SceneName;

    bool begunLoad = false;
}
