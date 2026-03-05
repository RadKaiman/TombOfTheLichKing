using UnityEngine;
using Zenject;
using UniRx;
using System;

public class LuckController : IInitializable, IDisposable
{
    private readonly PlayerModel _playerModel;
    private readonly LevelModel _levelModel;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public LuckController(PlayerModel playerModel, LevelModel levelModel)
    {
        _playerModel = playerModel;
        _levelModel = levelModel;
    }

    public void Initialize()
    {
        _levelModel.OnTileOpen
            .Subscribe(LuckChance)
            .AddTo(_disposables);
    }

    private void LuckChance(TileData tile)
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        if (roll > _playerModel.LuckChance.Value)
            return;

        Debug.Log("Удача сработала!");

        switch (tile.Type)
        {
            case TileType.Chest:
                // нужно будет удвоить золото
                break;
            case TileType.Skeletic:
                // нужно будет убрать урон от скелетика
                break;
            case TileType.Empty:
                // нужно будет дать ключ, если его ещё нет
                break;
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
