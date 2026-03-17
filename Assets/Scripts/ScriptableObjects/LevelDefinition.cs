using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Tomb/LevelDefinition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Grid Settings")]
    public int Width = 5;
    public int Height = 5;

    [Header("Turn Settings")]
    public int MinTurns = 8;
    public int MaxTurns = 12;

    [Header("Chances")]
    [Range(0, 100)] public int ChestChance = 30;
    [Range(0, 100)] public int SkeleticChance = 25;
    [Range(0, 100)] public int KeyChance = 5;

    [Header("Chest Settings")]
    public int MinGoldValue = 1;
    public int MaxGoldValue = 5;

    [Header("Skeletic Settings")]
    public int MinSkeleticDamage = 1;
    public int MaxSkeleticDamage = 3;

    [Header("Rewards")]
    public int CompletionGoldReward = 10;
    public int BonusGoldPerTurnLeft = 2;

    private void OnValidate()
    {
        int total = ChestChance + SkeleticChance + KeyChance;
        if (total > 100)
        {
            Debug.LogWarning($"Общий шанс превышает 100% ({total}%). Исправь!");
        }
    }
}
