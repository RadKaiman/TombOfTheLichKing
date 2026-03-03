using UnityEngine;
using Zenject;
using UniRx;
using System;

public class TurnController : IInitializable, IDisposable
{
    private readonly PlayerModel _playerModel;
    private readonly LevelModel _levelModel;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public TurnController(PlayerModel playerModel, LevelModel levelModel)
    {
        _playerModel = playerModel;
        _levelModel = levelModel;
    }

    public void Initialize()
    {
        _levelModel.OnTileOpen
            .Subscribe(_ => _playerModel.UseTurn())
            .AddTo(_disposables);

        _playerModel.CurrentTurn
            .Where(turn => turn >= _playerModel.MaxTurn.Value)
            .Subscribe(_ => GameOver())
            .AddTo(_disposables);
    }

    private void GameOver()
    {
        Debug.Log("Игра окончена: Не осталось ходов"); // потом реализовать панель поражения
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
