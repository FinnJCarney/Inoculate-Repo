using UnityEngine;

[CreateAssetMenu]
public class LevelInfo : ScriptableObject
{

    [SerializeField] public string LevelScene;
    [SerializeField] public string LevelName;
    [SerializeField] public Sprite LevelImage;
    [SerializeField] public string LevelDescription;
}
