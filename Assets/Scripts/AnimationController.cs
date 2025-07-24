using UnityEngine;

public class AnimationController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animation = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.speed = (1 / Time.timeScale);

        //if(!animation.isPlaying)
        //{
        //    animation.clip = clips[Mathf.RoundToInt(Random.Range(0, clips.Length - 1))];
        //    animation.Play();
        //}
    }

    private Animator animator;
    private Animation animation;

    [SerializeField] AnimationClip[] clips;
}
