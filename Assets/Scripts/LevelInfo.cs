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

    [SerializeField] public LevelInfo[] levelsToAdd;
    [SerializeField] public LevelInfo[] levelsToRemove;
}