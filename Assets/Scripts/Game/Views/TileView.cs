using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.EventSystems;

public class TileView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _closedSprite;
    [SerializeField] private Sprite _emptySprite;
    [SerializeField] private Sprite _chestSprite;
    [SerializeField] private Sprite _skeleticSprite;
    [SerializeField] private Sprite _keySprite;

    private TileData _tileData;
    private int _x, _y;

    public Subject<TileView> OnTileClicked { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        OnTileClicked = new Subject<TileView>();
    }

    public void Initialize(int x, int y, TileData data)
    {
        _x = x;
        _y = y;
        _tileData = data;
        UpdateView();
    }

    private void UpdateView()
    {
        if (!_tileData.IsOpen)
        {
            _iconImage.sprite = _closedSprite;
            return;
        }

        switch (_tileData.Type)
        {
            case TileType.Empty:
                _iconImage.sprite = _emptySprite; // нужно ещё добавить цифры сколько вокруг сокровищ 
                break;
            case TileType.Chest:
                _iconImage.sprite = _chestSprite;
                break;
            case TileType.Skeletic:
                _iconImage.sprite = _skeleticSprite;
                break;
            case TileType.Key:
                _iconImage.sprite = _keySprite;
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_tileData.IsOpen)
        {
            OnTileClicked.OnNext(this);
        }
    }
}
