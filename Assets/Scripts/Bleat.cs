using NUnit.Framework;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bleat : MonoBehaviour
{
    private void Start()
    {
        AudioManager.aM.PlayOneShot(bleatCreated, ScreenPlane.phoneScreen.transform);
    }

    public void SetBleatText(Sprite profImage, string bleater, string text)
    {
        bleaterProfile.sprite = profImage;
        bleaterName.text = bleater;
        bleatText.text = text;
    }

    public void CancelBleat()
    {
        AudioManager.aM.PlayOneShot(bleatFailed, ScreenPlane.phoneScreen.transform);
        bleatText.text = "Message Deleted";
    }

    public void CreateResponse(Node_UserInformation respondingNode)
    {
        GameObject newResponseObj = Instantiate<GameObject>(ResponsePrefab, responseHolder);
        ResponseObj newResponse = newResponseObj.GetComponent<ResponseObj>();

        newResponse.bleaterProfile.sprite = respondingNode.NodeImage;
        newResponse.bleaterName.text = respondingNode.NodeName;
        newResponse.bleatText.text = bleatResponses[Mathf.FloorToInt(Random.Range(0, bleatResponses.Length))].Text;

        AudioManager.aM.PlayOneShot(bleatCompleted, ScreenPlane.phoneScreen.transform);
    }

    [SerializeField] private Image bleaterProfile;
    [SerializeField] private TextMeshProUGUI bleaterName;
    [SerializeField] private TextMeshProUGUI bleatText;

    public Response[] bleatResponses;

    [SerializeField] GameObject ResponsePrefab;
    [SerializeField] Transform responseHolder;

    [SerializeField] AudioClip bleatCreated;
    [SerializeField] AudioClip bleatCompleted;
    [SerializeField] AudioClip bleatFailed;

}

