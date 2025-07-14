using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private void Start()
    {
        if (sM != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            sM = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public static SoundManager sM;
}
