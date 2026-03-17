using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using TMPro;
using System;

public class HudView : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthText;

    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _turnText;

    [Header("Key Status")]
    [SerializeField] private Image _keyIcon;
    [SerializeField] private Sprite _KeyNotFound;
    [SerializeField] private Sprite _KeyFound;

    [Header("Level Progress")]
    [SerializeField] private TMP_Text _levelText;

    [Header("Message")]
    [SerializeField] private TMP_Text _messageText;

    private PlayerModel _playerModel;
    private LevelModel _currentLevelModel;
    private LevelProgressionController _progressionController;
    private CompositeDisposable _disposables = new CompositeDisposable();

    [Inject]
    public void Construct(PlayerModel playerModel, LevelGenerationController levelGenController, LevelProgressionController progressionController)
    {
        _playerModel = playerModel;
        _progressionController = progressionController;

        levelGenController.OnLevelGeneratedAsObservable()
            .Subscribe(level =>
            {
                _currentLevelModel = level;
                SetupLevelSubscriptions(level);
                UpdateLevelDisplay();
            })
            .AddTo(_disposables);

        progressionController.OnLevelStarted
            .Subscribe(_ => UpdateLevelDisplay())
            .AddTo(_disposables);

        progressionController.OnLevelRestarted
            .Subscribe(_ => ShowMessage("Уровень перезапущен", Color.white))
            .AddTo(_disposables);

        progressionController.OnLevelCompleted
            .Subscribe(_ => ShowMessage("Уровень завершён", Color.green))
            .AddTo(_disposables);

        progressionController.OnGameCompleted
            .Subscribe(_ => ShowMessage("Игра пройдена!", Color.green))
            .AddTo(_disposables);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupPlayerSubscriptions();
        UpdateLevelDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupPlayerSubscriptions()
    {
        _playerModel.Health
            .CombineLatest(_playerModel.MaxHealth, (current, max) => new { current, max})
            .Subscribe(health =>
            {
                _healthSlider.value = (float)health.current / health.max;
                _healthText.text = $"{health.current}/{health.max}";
            })
            .AddTo(_disposables);

        _playerModel.Gold
            .Subscribe(gold => _goldText.text = gold.ToString())
            .AddTo(_disposables);
    }

    private void UpdateLevelDisplay()
    {
        if (_levelText != null)
        {
            int current = _progressionController.CurrentLevelIndex.Value + 1;
            int total = _progressionController.GetTotalLevelsCount();
            _levelText.text = $"Уровень {current}/{total}";
        }
    }

    private void SetupLevelSubscriptions(LevelModel level)
    {
        level.CurrentTurn
            .CombineLatest(level.MaxTurn, (current, max) => $"{current}/{max}")
            .Subscribe(turnText =>
            {
                if (_turnText != null)
                    _turnText.text = turnText;
            })
            .AddTo(_disposables);

        level.IsKeyFound
            .Subscribe(found =>
            {
                if (_keyIcon != null)
                    _keyIcon.sprite = found ? _KeyFound : _KeyNotFound;
            })
            .AddTo(_disposables);
    }

    private void ShowMessage(string message, Color color)
    {
        if (_messageText == null) return;

        _messageText.text = message;
        _messageText.color = color;
        _messageText.gameObject.SetActive(true);

        Observable.Timer(TimeSpan.FromSeconds(2))
            .Subscribe(_ => _messageText.gameObject.SetActive(false))
            .AddTo(_disposables);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }
}
