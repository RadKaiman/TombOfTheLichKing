using UnityEngine;
using System;
using UniRx;
using Zenject;

public class LevelModel
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TileData[,] Tiles { get; private set; }

    public BoolReactiveProperty IsKeyFound { get; private set; }

    public int TotalChests { get; private set; }

    public Subject<TileData> OnTileOpen { get; private set; }

    public LevelModel(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new TileData[width, height];
        IsKeyFound = new BoolReactiveProperty(false);
        OnTileOpen = new Subject<TileData>();
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

        Tiles[x, y].IsOpen = true;

        OnTileOpen.OnNext(Tiles[x, y]);

        if (Tiles[x, y].Type == TileType.Key)
            IsKeyFound.Value = true;

        return true;
    }

    public class Factory : PlaceholderFactory<int, int, LevelModel> { }
}

[Serializable]
public class TileData
{
    public TileType Type;
    public bool IsOpen;
    public int HintNumber;
    public int Value;

    public TileData(TileType type, int value = 0)
    {
        Type = type;
        Value = value;
        IsOpen = false;
        HintNumber = 0;
    }
}

public enum TileType
{
    Empty,
    Chest,
    Skeletic,
    Key
}
