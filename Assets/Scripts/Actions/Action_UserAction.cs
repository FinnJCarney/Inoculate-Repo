using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Action_UserAction : Action
{
    [SerializeField] string itemName;

    public virtual bool CheckUserActionAvailability()
    {
        //Check Faction Inventory
        //If item is present, check cost
        //If there and enough of it is there, yes
        return true;
    }

    public virtual bool PerformUserAction()
    {
        return false;
    }
}

