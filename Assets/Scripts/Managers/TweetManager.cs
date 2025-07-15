using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class TweetManager : MonoBehaviour
{
    private void Awake()
    {
        tM = this;
    }

    public int GetTweet(TweetInfo tweetInfo)
    {
        return Mathf.FloorToInt(Random.Range(0, tweetInfo.tweets.Count));
    }

    public Bleat PublishTweet(TweetInfo tweetInfo, Node_UserInformation actingNode, Node_UserInformation receiving, Faction actingFaction)
    {
        GameObject newBleatObj = Instantiate<GameObject>(bleatPrefab, bleatHolder);
        //Check what tweet the node should put out, for now we just do random
        int bleatNum = Mathf.FloorToInt(Random.Range(0, tweetInfo.tweets.Count));
        string bleatText = tweetInfo.tweets[bleatNum].Text;
        bleatText = bleatText.Replace("[FACTION]", LevelManager.lM.levelFactions[actingFaction].name);
        bleatText = bleatText.Replace("[RECEIVER]", receiving.NodeName);
        Bleat newBleat = newBleatObj.GetComponent<Bleat>();
        newBleat.SetBleatText(actingNode.NodeImage, actingNode.NodeName, bleatText);
        newBleat.bleatResponses = tweetInfo.tweets[bleatNum].responses;
        return newBleat;
    }

    public string GiveTweetText(TweetInfo tweetInfo, int tweetNum, Node_UserInformation actingNode, Node_UserInformation receiving, Faction actingFaction)
    {
        string tweetText = tweetInfo.tweets[Mathf.FloorToInt(Random.Range(0, tweetInfo.tweets.Count))].Text;
        tweetText = tweetText.Replace("[FACTION]", LevelManager.lM.levelFactions[actingFaction].name);
        tweetText = tweetText.Replace("[RECEIVER]", receiving.NodeName);
        return tweetText;
    }

    public GameObject bleatPrefab;
    public Transform bleatHolder; 

    public static TweetManager tM;
}
