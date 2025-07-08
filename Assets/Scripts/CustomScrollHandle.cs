using UnityEngine;

public class CustomScrollHandle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        boxCollider.size = new Vector3(rectTransform.rect.size.x, rectTransform.rect.size.y, 2f);
    }

    public CustomScrollView cSV;

    private RectTransform rectTransform;
    private BoxCollider boxCollider;
}
