using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TweetInfo : ScriptableObject
{
    public List<Tweet> tweets = new List<Tweet>();
}

[System.Serializable]
public struct Tweet
{
    public string Text;
    public Personality_NO pNO;
    public Personality_TF pTF;
    public Personality_Special pSpecial;
    public Response[] responses;
}

[System.Serializable]
public struct Response
{
    public string Text;
    public Personality_NO pNO;
    public Personality_TF pTF;
    public Personality_Special pSpecial;
}

