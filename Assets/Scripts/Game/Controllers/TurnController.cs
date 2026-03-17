using UnityEngine;
using Zenject;
using UniRx;
using System;

public class TurnController : IInitializable, IDisposable
{
    private readonly LevelGenerationController _levelGenController;
    private readonly LevelProgressionController _progressionController;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    public TurnController(LevelGenerationController levelGenController, LevelProgressionController progressionController)
    {
        _levelGenController = levelGenController;
        _progressionController = progressionController;
    }

    public void Initialize()
    {
        _levelGenController.OnLevelGenerated
            .Subscribe(level =>
            {
                SetupLevelSubscriptions(level);
            })
            .AddTo(_disposables);
    }

    private void SetupLevelSubscriptions(LevelModel level)
    {
        level.CurrentTurn
            .CombineLatest(level.MaxTurn, (current, max) => new { current, max })
            .Where(x => x.current >= x.max && !level.IsLevelCompleted.Value)
            .Subscribe(_ => NoTurnsLeft(level))
            .AddTo(_disposables);
    }

    private void NoTurnsLeft(LevelModel level)
    {
        if (level != null && level.IsKeyFound.Value)
        {
            Debug.Log("Ключ найден на последнем ходу!");
            return;
        }

        Observable.Timer(TimeSpan.FromSeconds(1.0f))
            .Subscribe(_ => _progressionController.RestartLevel())
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
