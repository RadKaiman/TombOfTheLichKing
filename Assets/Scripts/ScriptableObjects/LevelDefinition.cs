using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Tomb/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    public int Width = 5;
    public int Height = 5;
    public int Turns = 8;

    [Range(0, 100)] public int ChestChance = 30;
    [Range(0, 100)] public int SkeleticChance = 25;
    [Range(0, 100)] public int KeyChance = 5;

    public int MinGoldValue = 1;
    public int MaxGoldValue = 5;
    public int MinSkeleticDamage = 1;
    public int MaxSkeleticDamage = 3;
}
