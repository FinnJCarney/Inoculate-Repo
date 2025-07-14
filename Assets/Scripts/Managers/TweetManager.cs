using DG.Tweening;
using UnityEngine;

public class TweetManager : MonoBehaviour
{
    private void Awake()
    {
        tM = this;
    }

    public void PublishTweet(TweetInfo tweetInfo, Node_UserInformation actingNode, Node_UserInformation receiving, Faction actingFaction)
    {
        //Check what tweet the node should put out, for now we just do random
        string tweetText = tweetInfo.tweets[Mathf.FloorToInt(Random.Range(0, tweetInfo.tweets.Count))].Text;
        tweetText = tweetText.Replace("[FACTION]", LevelManager.lM.levelFactions[actingFaction].name);
        tweetText = tweetText.Replace("[RECEIVER]", receiving.NodeName);
        Debug.Log("Tweet from " + actingNode.NodeName + ": " + tweetText);
    }

    public static TweetManager tM;
}
