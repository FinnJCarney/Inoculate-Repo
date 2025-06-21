using UnityEngine;

public class Sun : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(TimeManager.tM.adjustedDeltaTime * -0.5f, 0, 0);
    }
}
