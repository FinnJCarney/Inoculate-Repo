using UnityEngine;
using UnityEngine.UI;

public class CustomScrollView : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ResetHandle();
    }

    // Update is called once per frame
    void Update()
    {
        SetHandleSize();
        CheckHandle();
    }

    public void MoveHandle(float velocity)
    {
        if(content.rect.height < myRectTransform.rect.height)
        {
            return;
        }

        float newScrollHandlePosY = Mathf.Clamp(scrollbarHandle.localPosition.y + velocity, scrollbarBackground.rect.yMin + (scrollbarHandle.rect.height / 2f), scrollbarBackground.rect.yMax - (scrollbarHandle.rect.height / 2f));
        float changeAmount = newScrollHandlePosY - scrollbarHandle.localPosition.y;
        
        scrollbarHandle.SetLocalPositionAndRotation(new Vector3(0, newScrollHandlePosY, 0), Quaternion.Euler(0, 0, 0));

        float amountThroughContent = 0.5f - (newScrollHandlePosY / ((scrollbarBackground.rect.yMax * 2) - scrollbarHandle.rect.height));

        float newContentY = myRectTransform.rect.height + ((content.rect.height - myRectTransform.rect.height) * amountThroughContent);
        content.SetLocalPositionAndRotation(new Vector3(0, newContentY), Quaternion.Euler(0, 0, 0));

    }

    private void CheckHandle()
    {
        float amountThroughContent = 0.5f - (scrollbarHandle.localPosition.y / ((scrollbarBackground.rect.yMax * 2) - scrollbarHandle.rect.height));

        if(amountThroughContent > 0.9f)
        {
            amountThroughContent = 1f;
            float newContentY = myRectTransform.rect.height + ((content.rect.height - myRectTransform.rect.height) * amountThroughContent);
            content.SetLocalPositionAndRotation(new Vector3(0, newContentY), Quaternion.Euler(0, 0, 0));
        }

    }

    void SetHandleSize()
    {
        float handleHeight = Mathf.Min(1f, (myRectTransform.rect.height / content.rect.height)) * scrollbarBackground.rect.height;
        scrollbarHandle.sizeDelta = new Vector2(scrollbarHandle.sizeDelta.x, handleHeight);
    }

    public void ResetHandle()
    {
        SetHandleSize();
        scrollbarHandle.SetLocalPositionAndRotation(new Vector3(0, scrollbarBackground.rect.yMax - (scrollbarHandle.rect.height / 2), 0), Quaternion.Euler(0, 0, 0));
        content.SetLocalPositionAndRotation(new Vector3(0, myRectTransform.rect.height), Quaternion.Euler(0, 0, 0));
    }

    private RectTransform myRectTransform;
    [SerializeField] RectTransform content;

    [SerializeField] RectTransform scrollbarBackground;
    [SerializeField] RectTransform scrollbarHandle;
}
