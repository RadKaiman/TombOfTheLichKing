using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using TMPro;

public class HudView : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _turnText;

    private PlayerModel _playerModel;

    [Inject]
    public void Construct(PlayerModel playerModel)
    {
        _playerModel = playerModel;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerModel.Health
            .Subscribe(health =>_healthSlider.value = (float)health / _playerModel.MaxHealth.Value)
            .AddTo(this);

        _playerModel.Gold
            .Subscribe(gold => _goldText.text = gold.ToString())
            .AddTo(this);

        _playerModel.CurrentTurn
            .CombineLatest(_playerModel.MaxTurn, (current, max) => $"{current}/{max}")
            .Subscribe(turnText => _turnText.text = turnText)
            .AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
