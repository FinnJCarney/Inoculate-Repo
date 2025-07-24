using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        aM = this;
    }

    private void OnDestroy()
    {
        aM = null;
    }

    public void PlayOneShot(AudioClip audioClip, Transform parentTransform)
    {
        var newAudioObj = Instantiate(audioOneShotObj, parentTransform);
        newAudioObj.GetComponent<AudioSource>().pitch = Random.Range(0.85f, 1.15f);
        newAudioObj.GetComponent<AudioSource>().PlayOneShot(audioClip);
        audioObjs.Add(newAudioObj);
    }

    

    public static AudioManager aM;

    [SerializeField] private GameObject audioOneShotObj;

    public List<GameObject> audioObjs = new List<GameObject>();
}
