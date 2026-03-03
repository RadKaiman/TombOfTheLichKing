using UnityEngine;
using System;
using UniRx;

[Serializable]
public class PlayerModel
{
    public ReactiveProperty<int> MaxHealth { get; private set; }
    public ReactiveProperty<int> Health { get; private set; }
    public ReactiveProperty<int> Gold { get; private set; }
    public ReactiveProperty<int> CurrentTurn { get; private set; }
    public ReactiveProperty<int> MaxTurn { get; private set; }

    public IntReactiveProperty VitalityLevel { get; private set; }
    public IntReactiveProperty LuckLevel { get; private set; }
    public IntReactiveProperty PerceptionLevel { get; private set; }

    public IReadOnlyReactiveProperty<float> LuckChance { get; private set; }
    public IReadOnlyReactiveProperty<int> HealthRestorePerLevel { get; private set; }

    public PlayerModel()
    {
        MaxHealth = new ReactiveProperty<int>(10);
        Health = new ReactiveProperty<int>(10);
        Gold = new ReactiveProperty<int>(0);
        CurrentTurn = new ReactiveProperty<int>(0);
        MaxTurn = new ReactiveProperty<int>(15);

        VitalityLevel = new IntReactiveProperty(1);
        LuckLevel = new IntReactiveProperty(1);
        PerceptionLevel = new IntReactiveProperty(1);

        LuckChance = LuckLevel
            .Select(level => Mathf.Clamp(0.05f + level * 0.02f, 0f, 0.5f))// формулу просчёта потом забалансить
            .ToReadOnlyReactiveProperty();

        HealthRestorePerLevel = VitalityLevel
            .Select(vit => 2 + vit * 1)// формулу просчёта потом забалансить
            .ToReadOnlyReactiveProperty();
    }

    public void TakeDamage(int damage)
    {
        Health.Value = Mathf.Max(0, Health.Value - damage);
    }

    public void Heal(int amount)
    {
        Health.Value = Mathf.Min(MaxHealth.Value, Health.Value + amount);
    }

    public void AddGold(int amount)
    {
        Gold.Value += amount;
    }

    public void ReduceGold(int amount)
    {
        Gold.Value -= amount;
    }

    public bool CanBuy(int amount)
    {
        return Gold.Value >= amount;
    }

    public void UseTurn()
    {
        CurrentTurn.Value++;
    }

    public bool CanMakeTurn()
    {
        return CurrentTurn.Value < MaxTurn.Value;
    }

    public void ResetTurns()
    {
        CurrentTurn.Value = 0;
    }

    public void RestoreHealthBetweenLevels()
    {
        Heal(HealthRestorePerLevel.Value);
    }
}
