using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;

public class ShopModel
{
    public ReactiveProperty<int> PlayerGold { get; private set; }
    public ReactiveDictionary<string, int> ConsumablesInventory { get; private set; }

    public List<ShopItem> AvailableItems { get; private set; }

    public ShopModel(PlayerModel playerModel)
    {
        PlayerGold = playerModel.Gold;
        ConsumablesInventory = new ReactiveDictionary<string, int>();

        InitializeShop();
    }

    private void InitializeShop()
    {
        AvailableItems = new List<ShopItem>
        {
            new ShopItem { Id = "health_potion", Name = "Зелье лечения", Price = 10, IsConsumable = true },
            new ShopItem { Id = "turn_elixir", Name = "Эликсир ходов", Price = 15, IsConsumable = true },
            new ShopItem { Id = "luck_amulet", Name = "Амулет удачи", Price = 20, IsConsumable = true },
            new ShopItem { Id = "vitality_up", Name = "Прокачка тела", Price = 30, IsConsumable = false, StatToUpgrade = "Vitality" },
            new ShopItem { Id = "luck_up", Name = "Прокачка удачи", Price = 25, IsConsumable = false, StatToUpgrade = "Luck" },
            new ShopItem { Id = "perception_up", Name = "Прокачка внимания", Price = 20, IsConsumable = false, StatToUpgrade = "Perception" }
        };
    }

    public bool TryBuy(ShopItem item, PlayerModel playerModel)
    {
        if (PlayerGold.Value < item.Price)
            return false;

        PlayerGold.Value -= item.Price;

        if (item.IsConsumable)
        {
            if (ConsumablesInventory.ContainsKey(item.Id))
                ConsumablesInventory[item.Id]++;
            else
                ConsumablesInventory.Add(item.Id, 1);
        }
        else
        {
            switch (item.StatToUpgrade)
            {
                case "Vitality":
                    playerModel.VitalityLevel.Value++;
                    break;
                case "Luck":
                    playerModel.LuckLevel.Value++;
                    break;
                case "Perception":
                    playerModel.PerceptionLevel.Value++;
                    break;
            }
        }

        return true;
    }
}

[Serializable]
public class ShopItem
{
    public string Id;
    public string Name;
    public int Price;
    public bool IsConsumable;

    public string StatToUpgrade;
    public int UpgradeAmount = 1;
}
