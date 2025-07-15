using UnityEngine;
using UnityEngine.UI;

public class BleatHolder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<VerticalLayoutGroup>().spacing = 1f;
        GetComponent<VerticalLayoutGroup>().padding.top = 1;
        GetComponent<VerticalLayoutGroup>().spacing = 0f;
        GetComponent<VerticalLayoutGroup>().padding.top = 0;
    }
}
