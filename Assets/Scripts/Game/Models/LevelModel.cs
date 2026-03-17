using UnityEngine;
using System;
using UniRx;
using Zenject;

public class LevelModel
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TileData[,] Tiles { get; private set; }

    public ReactiveProperty<int> CurrentTurn { get; private set; }
    public ReactiveProperty<int> MaxTurn { get; private set; }

    public BoolReactiveProperty IsKeyFound { get; private set; }
    public BoolReactiveProperty IsLevelCompleted { get; private set; }

    public struct TileOpenInfo
    {
        public int x;
        public int y;
        public TileData tileData;
    }

    public int TotalChests { get; private set; }

    public Subject<TileOpenInfo> OnTileOpen { get; private set; }

    public LevelModel(int width, int height, int maxTurns)
    {
        Width = width;
        Height = height;
        Tiles = new TileData[width, height];

        CurrentTurn = new ReactiveProperty<int>(0);
        MaxTurn = new ReactiveProperty<int>(maxTurns);
        IsKeyFound = new BoolReactiveProperty(false);
        IsLevelCompleted = new BoolReactiveProperty(false);

        OnTileOpen = new Subject<TileOpenInfo>();
    }


    public void SetTile(int x, int y, TileData data)
    {
        Tiles[x, y] = data;
        if (data.Type == TileType.Chest)
            TotalChests++;
    }

    public bool TryOpenTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
        if (Tiles[x, y].IsOpen) return false;
        if (!CanMakeTurn()) return false;

        Tiles[x, y].IsOpen = true;
        UseTurn();

        OnTileOpen.OnNext(new TileOpenInfo { x = x, y = y, tileData = Tiles[x, y] });

        if (Tiles[x, y].Type == TileType.Key)
        {
            IsKeyFound.Value = true;
            CompleteLevel();
        }

        return true;
    }

    private void CompleteLevel()
    {
        IsLevelCompleted.Value = true;
    }

    public bool CanMakeTurn()
    {
        return CurrentTurn.Value < MaxTurn.Value && !IsLevelCompleted.Value;
    }

    public void UseTurn()
    {
        if (CanMakeTurn())
        {
            CurrentTurn.Value++;
        }
    }

    public void ResetTurns()
    {
        CurrentTurn.Value = 0;
    }

    public class Factory : PlaceholderFactory<int, int, int, LevelModel> { }
}

[Serializable]
public class TileData
{
    public TileType Type;
    public bool IsOpen;
    public int ChestCount;
    public int SkeleticCount;
    public int Value;

    public TileData(TileType type, int value = 0)
    {
        Type = type;
        Value = value;
        IsOpen = false;
        ChestCount = 0;
        SkeleticCount = 0;
    }
}

public enum TileType
{
    Empty,
    Chest,
    Skeletic,
    Key
}
