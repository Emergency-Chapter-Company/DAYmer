using UnityEngine;

// 2D 아이템 데이터

[CreateAssetMenu(fileName = "ItemData2D", menuName = "RoomDecoration2D/ItemData")]
public class ItemData2D : ScriptableObject
{
    [SerializeField] private string itemName = "New Item";
    [SerializeField] private int itemID = 0;
    [SerializeField] private Sprite sprite;
    [SerializeField] private ItemType placementType = ItemType.Floor;
    [SerializeField] private Color itemColor = Color.white;

    public string ItemName => itemName;
    public int ItemID => itemID;
    public Sprite Sprite => sprite;
    public ItemType PlacementType => placementType;
    public Color ItemColor => itemColor;
}

// 배치 가능 아이템 유형

public class RoomDecoItem : MonoBehaviour
{
    [SerializeField] private ItemData2D itemData;
    [SerializeField] private bool isPlaced = false;
    [SerializeField] private Vector2Int gridPosition;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetPlaced(bool placed)
    {
        isPlaced = placed;
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    public void SetSortingOrder(int order)
    {
        if(spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = order;
        }
    }

    public string GetItemName() => itemData != null ? itemData.ItemName : "Unknown";
    public bool IsPlaced() => isPlaced;
    public Vector2Int GetGridPosition() => gridPosition;
    public void SetItemData(ItemData2D data) => itemData = data;
}
