using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.EventSystems;
using Zenject;
using TMPro;

public class TileView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _iconImage;

    [Header("Counters")]
    [SerializeField] private TMP_Text _chestText;
    [SerializeField] private TMP_Text _skeleticText;

    [Header("Sprites")]
    [SerializeField] private Sprite _closedSprite;
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private Sprite _chestSprite;
    [SerializeField] private Sprite _skeleticSprite;
    [SerializeField] private Sprite _keySprite;

    private TileData _tileData;
    private int _x, _y;

    public int X => _x;
    public int Y => _y;

    public Subject<TileView> OnTileClicked { get; private set; } = new Subject<TileView>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Inject]
    public void Construct(int x, int y, TileData data)
    {
        _x = x;
        _y = y;
        _tileData = data;
        UpdateView();
    }

    public void UpdateView()
    {
        if (_iconImage == null) return;

        if (!_tileData.IsOpen)
        {
            _iconImage.sprite = _closedSprite;
            HideCounters();
            return;
        }

        switch (_tileData.Type)
        {
            case TileType.Empty:
                _iconImage.sprite = _emptySprite;
                ShowCounters();
                break;
            case TileType.Chest:
                _iconImage.sprite = _chestSprite;
                HideCounters();
                break;
            case TileType.Skeletic:
                _iconImage.sprite = _skeleticSprite;
                HideCounters();
                break;
            case TileType.Key:
                _iconImage.sprite = _keySprite;
                HideCounters();
                break;
        }
    }

    private void ShowCounters()
    {
        if (_chestText != null)
        {
            if (_tileData.ChestCount > 0)
            {
                _chestText.text = _tileData.ChestCount.ToString();
                _chestText.gameObject.SetActive(true);
            }
            else
            {
                _chestText.gameObject.SetActive(false);
            }
        }

        if (_skeleticText != null)
        {
            if (_tileData.SkeleticCount > 0)
            {
                _skeleticText.text = _tileData.SkeleticCount.ToString();
                _skeleticText.gameObject.SetActive(true);
            }
            else
            {
                _skeleticText.gameObject.SetActive(false);
            }
        }
    }

    private void HideCounters()
    {
        if (_chestText != null)
            _chestText.gameObject.SetActive(false);
        if (_skeleticText != null)
            _skeleticText.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_tileData.IsOpen)
        {
            OnTileClicked.OnNext(this);
        }
    }

    private void OnDestroy()
    {
        OnTileClicked?.Dispose();
    }

    public class Factory : PlaceholderFactory<int, int, TileData, TileView> { }
}
