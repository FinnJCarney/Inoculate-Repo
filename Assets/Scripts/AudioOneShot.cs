using UnityEngine;

public class AudioOneShot : MonoBehaviour
{
    private void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        if(!myAudioSource.isPlaying)
        {
            AudioManager.aM.audioObjs.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
    }

    private AudioSource myAudioSource; 
}
