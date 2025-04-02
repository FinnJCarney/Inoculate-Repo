using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private void Awake()
    {
        if(lM != null)
        {
            Destroy(this);
        }
        else
        {
            lM = this;
        }
    }

    private void OnDestroy()
    {
        lM = null;
    }

    public Node playerNode;
    public AllyStatus playerAllyFaction;

    public static LevelManager lM;
}
