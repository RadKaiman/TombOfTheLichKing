using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;
using Zenject;

public class LevelProgressionController : IInitializable, IDisposable
{
    private readonly PlayerModel _playerModel;
    private readonly LevelGenerationController _levelGenController;
    private readonly string _startingLevelName;

    private List<string> _levelOrder = new List<string>
    {
        "Level_01_Tutorial",
        "Level_02_Easy",
        "Level_03_Medium",
        "Level_04_Hard",
        "Level_05_Expert"
    };

    private ReactiveProperty<int> _currentLevelIndex = new ReactiveProperty<int>(0);
    private CompositeDisposable _disposables = new CompositeDisposable();
    private LevelDefinition _currentLevelDefinition;

    public IReadOnlyReactiveProperty<int> CurrentLevelIndex => _currentLevelIndex;
    public Subject<Unit> OnLevelStarted { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnLevelRestarted { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnLevelCompleted { get; private set; } = new Subject<Unit>();
    public Subject<Unit> OnGameCompleted { get; private set; } = new Subject<Unit>();

    [Inject]
    public LevelProgressionController( PlayerModel playerModel, LevelGenerationController levelGenController, string startingLevelName)
    {
        _playerModel = playerModel;
        _levelGenController = levelGenController;
        _startingLevelName = startingLevelName;
    }

    public void Initialize()
    {
        Debug.Log($"Стартовый уровень: {_startingLevelName}");

        int startIndex = _levelOrder.IndexOf(_startingLevelName);
        if (startIndex >= 0)
        {
            _currentLevelIndex.Value = startIndex;
        }

        SetupSubscriptions();

        LoadCurrentLevel();
    }

    private void SetupSubscriptions()
    {
        _levelGenController.OnLevelGeneratedAsObservable()
            .Subscribe(_ =>
            {
                var level = _levelGenController.GetCurrentLevel();
                if (level != null)
                {
                    level.IsKeyFound
                        .Where(found => found)
                        .Subscribe(_ =>
                        {
                            Observable.Timer(TimeSpan.FromSeconds(0.5f))
                                .Subscribe(__ => CompleteCurrentLevel())
                                .AddTo(_disposables);
                        })
                        .AddTo(_disposables);
                }
            })
            .AddTo(_disposables);

        _playerModel.Health
            .Where(health => health <= 0)
            .Subscribe(_ => OnPlayerDeath())
            .AddTo(_disposables);
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Игрок умер! Перезапуск уровня...");
        Observable.Timer(TimeSpan.FromSeconds(1.5f))
            .Subscribe(_ => RestartLevel())
            .AddTo(_disposables);
    }

    public void LoadCurrentLevel()
    {
        string levelName = _levelOrder[_currentLevelIndex.Value];
        _currentLevelDefinition = Resources.Load<LevelDefinition>($"GameData/Levels/{levelName}");

        if (_currentLevelDefinition != null)
        {
            _levelGenController.GenerateLevel(_currentLevelDefinition);
            Debug.Log($"Загружен уровень: {levelName} (Номер: {_currentLevelIndex.Value})");
            OnLevelStarted.OnNext(Unit.Default);
        }
        else
        {
            Debug.LogError($"Не найден уровень с именем: {levelName}");
        }
    }

    public void CompleteCurrentLevel()
    {
        Debug.Log($"Уровень {_currentLevelIndex.Value} завершён!");

        LevelCompletionReward();

        OnLevelCompleted.OnNext(Unit.Default);

        Observable.Timer(TimeSpan.FromSeconds(3))
                .Subscribe(_ => NextLevel())
                .AddTo(_disposables);
    }

    private void LevelCompletionReward()
    {
        if (_currentLevelDefinition == null)
        {
            Debug.LogError("Нельзя дать награду: нет текущего уровня!");
            return;
        }

        var currentLevel = _levelGenController.GetCurrentLevel();
        if (currentLevel == null) return;

        int baseReward = _currentLevelDefinition.CompletionGoldReward;

        int turnsLeft = currentLevel.MaxTurn.Value - currentLevel.CurrentTurn.Value;
        int turnBonus = turnsLeft * _currentLevelDefinition.BonusGoldPerTurnLeft;

        int totalReward = baseReward + turnBonus;

        _playerModel.AddGold(totalReward);

        Debug.Log($"Уровень завершён! Награда: {totalReward} золота ");
    }

    public void NextLevel()
    {
        if (_currentLevelIndex.Value + 1 < _levelOrder.Count)
        {
            _currentLevelIndex.Value++;

            _playerModel.RestoreHealthBetweenLevels();

            LoadCurrentLevel();
        }
        else
        {
            Debug.Log("Игра пройдена!");
            OnGameCompleted.OnNext(Unit.Default);
        }
    }

    public void RestartLevel()
    {
        Debug.Log("Перезагрузка уровня...");

        _playerModel.Heal(5);

        LoadCurrentLevel();

        OnLevelRestarted.OnNext(Unit.Default);
    }

    public void GoToLevel(int index)
    {
        if (index >= 0 && index < _levelOrder.Count)
        {
            _currentLevelIndex.Value = index;
            LoadCurrentLevel();
        }
    }

    public string GetCurrentLevelName()
    {
        return _levelOrder[_currentLevelIndex.Value];
    }

    public int GetTotalLevelsCount()
    {
        return _levelOrder.Count;
    }

    public LevelDefinition GetCurrentLevelDefinition()
    {
        return _currentLevelDefinition;
    }

    public void Dispose()
    {
        _disposables.Dispose();
        OnLevelStarted?.Dispose();
        OnLevelRestarted?.Dispose();
        OnLevelCompleted?.Dispose();
        OnGameCompleted?.Dispose();
    }
}
