using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

public class LevelGenerationController : IDisposable
{
    private readonly PlayerModel _playerModel;
    private readonly LevelModel.Factory _levelFactory;
    private readonly TileView.Factory _tileFactory;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private LevelModel _currentLevel;
    private List<TileView> _activeTiles = new List<TileView>();

    public Subject<LevelModel> OnLevelGenerated { get; private set; } = new Subject<LevelModel>();
    public Subject<Unit> OnLevelCompleted { get; private set; } = new Subject<Unit>();

    public LevelGenerationController(PlayerModel playerModel, LevelModel.Factory levelFactory, TileView.Factory tileFactory)
    {
        _playerModel = playerModel;
        _levelFactory = levelFactory;
        _tileFactory = tileFactory;
    }

    public LevelModel GenerateLevel(LevelDefinition definition)
    {
        int maxTurns = UnityEngine.Random.Range(
            definition.MinTurns,
            definition.MaxTurns + 1
        );

        var model = _levelFactory.Create(definition.Width, definition.Height, maxTurns);

        PlaceKey(model, definition);

        FillRemainingTiles(model, definition);

        CalculateHints(model);

        SubscribeToLevelEvents(model);

        SpawnTiles(model);

        _currentLevel = model;

        OnLevelGenerated.OnNext(model);

        return model;
    }

    private void SubscribeToLevelEvents(LevelModel model)
    {
        model.IsLevelCompleted
            .Where(completed => completed)
            .Subscribe(_ =>
            {
                OnLevelCompleted.OnNext(Unit.Default);
            })
            .AddTo(_disposables);
    }

    private void HandleTileOpen(TileData tileData)
    {
        Debug.Log($"Открыт тайл: {tileData.Type}");

        if (tileData.Type == TileType.Key)
        {
            CheckLevelEnd();
        }
    }

    private void CheckLevelEnd()
    {
        if (_currentLevel != null && _currentLevel.IsKeyFound.Value)
        {
            Debug.Log("Ключ найден, уровень завершён");
            OnLevelCompleted.OnNext(Unit.Default);

            Observable.Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ => EndLevel())
                .AddTo(_disposables);
        }
    }

    private void EndLevel()
    {
        Debug.Log("Завершение уровня"); // нужно добавить переход к магазину
    }

    private void PlaceKey(LevelModel model, LevelDefinition def)
    {
        int x = UnityEngine.Random.Range(0, def.Width);
        int y = UnityEngine.Random.Range(0, def.Height);
        model.SetTile(x, y, new TileData(TileType.Key, 0));
    }

    private void FillRemainingTiles(LevelModel model, LevelDefinition def)
    {
        for (int x = 0; x < def.Width; x++)
        {
            for (int y = 0; y < def.Height; y++)
            {
                if (model.Tiles[x, y] != null) continue;

                int roll = UnityEngine.Random.Range(0, 100);
                TileType type;
                int value = 0;

                if (roll < def.ChestChance)
                {
                    type = TileType.Chest;
                    value = UnityEngine.Random.Range(def.MinGoldValue, def.MaxGoldValue + 1);
                }
                else if (roll < def.ChestChance + def.SkeleticChance)
                {
                    type = TileType.Skeletic;
                    value = UnityEngine.Random.Range(def.MinSkeleticDamage, def.MaxSkeleticDamage + 1);
                }
                else if (roll < def.ChestChance + def.SkeleticChance + def.KeyChance)
                {
                    type = TileType.Key;
                }
                else
                {
                    type = TileType.Empty;
                }

                model.SetTile(x, y, new TileData(type, value));
            }
        }
    }

    private void CalculateHints(LevelModel model)
    {
        for (int x = 0; x < model.Width; x++)
        {
            for (int y = 0; y < model.Height; y++)
            {
                int chestCount = 0;
                int skeleticCount = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < model.Width && ny >= 0 && ny < model.Height)
                        {
                            TileType neighborType = model.Tiles[nx, ny].Type;
                            if (neighborType == TileType.Chest)
                                chestCount++;
                            else if (neighborType == TileType.Skeletic)
                                skeleticCount++;
                        }
                    }
                }

                model.Tiles[x, y].ChestCount = chestCount;
                model.Tiles[x, y].SkeleticCount = skeleticCount;
            }
        }
    }

    private void SpawnTiles(LevelModel model)
    {
        ClearOldTiles();

        for (int x = 0; x < model.Width; x++)
        {
            for (int y = 0; y < model.Height; y++)
            {
                SpawnTile(x, y, model.Tiles[x, y]);
            }
        }
    }

    private void SpawnTile(int x, int y, TileData tileData)
    {
        var tile = _tileFactory.Create(x, y, tileData);

        tile.OnTileClicked
            .Subscribe(clickedTile => HandleTileClick(clickedTile))
            .AddTo(tile)
            .AddTo(_disposables);

        _activeTiles.Add(tile);
    }

    private void HandleTileClick(TileView tile)
    {
        if (_currentLevel == null) return;

        bool open = _currentLevel.TryOpenTile(tile.X, tile.Y);

        if (open)
        {
            tile.UpdateView();
            ApplyTileEffect(_currentLevel.Tiles[tile.X, tile.Y]);
        }
    }

    private void ClearOldTiles()
    {
        foreach (var tile in _activeTiles)
        {
            if (tile != null)
            {
                GameObject.Destroy(tile.gameObject);
            }
        }
        _activeTiles.Clear();
    }

    private void ApplyTileEffect(TileData tile)
    {
        switch (tile.Type)
        {
            case TileType.Chest:
                _playerModel.AddGold(tile.Value);
                break;
            case TileType.Skeletic:
                _playerModel.TakeDamage(tile.Value);
                break;
        }
    }

    public LevelModel GetCurrentLevel()
    {
        return _currentLevel;
    }

    public IObservable<LevelModel> OnLevelGeneratedAsObservable()
    {
        return OnLevelGenerated.AsObservable();
    }

    public IObservable<Unit> OnLevelCompletedAsObservable()
    {
        return OnLevelCompleted.AsObservable();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        OnLevelGenerated?.Dispose();
        OnLevelCompleted?.Dispose();
    }
}
