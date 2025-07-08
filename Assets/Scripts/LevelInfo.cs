using UnityEngine;

[CreateAssetMenu]
public class LevelInfo : ScriptableObject
{

    [SerializeField] public string LevelScene;
    [SerializeField] public string LevelName;
    [SerializeField] public string LevelContact;
    [SerializeField] public Sprite LevelImage;
    [SerializeField] public string LevelDescription;
    [SerializeField] public GameMode LevelGameMode;
    [SerializeField] public int LevelMoney;
    [SerializeField] public Color clientColor;

    [SerializeField] public MessageContent[] chatMessages;

    [SerializeField] public LevelInfo[] levelsToAdd;
    [SerializeField] public LevelInfo[] levelsToRemove;
}

[System.Serializable]
public struct MessageContent
{
    public string Text;
    public bool clientText;
    public float timeAfterText;
}
