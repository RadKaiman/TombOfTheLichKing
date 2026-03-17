using UnityEngine;
using Zenject;
using UniRx;
using System;

public class LuckController : IInitializable, IDisposable
{
    private readonly PlayerModel _playerModel;
    private readonly LevelGenerationController _levelGenController;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private LevelModel _currentLevelModel;

    public LuckController(PlayerModel playerModel, LevelGenerationController levelGenController)
    {
        _playerModel = playerModel;
        _levelGenController = levelGenController;
    }

    public void Initialize()
    {
        _levelGenController.OnLevelGeneratedAsObservable()
            .Subscribe(_ =>
            {
                _currentLevelModel = _levelGenController.GetCurrentLevel();
                SetupLevelSubscriptions();
            })
            .AddTo(_disposables);
    }

    private void SetupLevelSubscriptions()
    {
        if (_currentLevelModel == null) return;

        _currentLevelModel.OnTileOpen
            .Subscribe(openInfo => LuckChance(openInfo))
            .AddTo(_disposables);
    }

    private void LuckChance(LevelModel.TileOpenInfo openInfo)
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        if (roll > _playerModel.LuckChance.Value)
            return;

        Debug.Log("Удача сработала!");

        var tile = openInfo.tileData;

        switch (tile.Type)
        {
            case TileType.Chest:
                tile.Value *= 2;
                break;
            case TileType.Skeletic:
                tile.Value = 0;
                break;
            case TileType.Empty:
                if (!_currentLevelModel.IsKeyFound.Value)
                {
                    _currentLevelModel.IsKeyFound.Value = true;
                }
                break;
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
