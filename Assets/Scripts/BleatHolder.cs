using UnityEngine;
using UnityEngine.UI;

public class BleatHolder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vlg = GetComponent<VerticalLayoutGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        vlg.spacing = 1f;
        vlg.padding.top = 1;
        vlg.spacing = spacing;
        vlg.padding.top = padding;
    }

    private VerticalLayoutGroup vlg;

    [SerializeField] private float spacing;
    [SerializeField] private int padding;
}
