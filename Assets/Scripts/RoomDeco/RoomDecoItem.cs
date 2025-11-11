using UnityEngine;

// 2D 아이템 데이터

//[CreateAssetMenu(fileName = "ItemData2D", menuName = "RoomDecoration2D/ItemData")]
//public class ItemData2D : ScriptableObject
//{
//    [SerializeField] private string itemName = "New Item";
//    [SerializeField] private int itemID = 0;
//    [SerializeField] private Sprite sprite;
//    [SerializeField] private GameObject itemPrefab;
//    [SerializeField] private ItemType placementType = ItemType.Floor;
//    [SerializeField] private Color itemColor = Color.white;

//    // 최대 배치 가능 갯수 (하려다가 말음)
//    // [SerializeField] private int maxCount = 5;

//    public string ItemName => itemName;
//    public int ItemID => itemID;
//    public Sprite Sprite => sprite;
//    public GameObject ItemPrefab => itemPrefab; // 편집모드용 프리팹
//    public ItemType PlacementType => placementType;
//    public Color ItemColor => itemColor;
//    // public int MaxCount => maxCount;
//}

// 배치 가능 아이템 유형

public class RoomDecoItem : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite sprite;
    private Color itemColor;

    /* ====== 아이템 속성 ====== */
    [Header("Item Data")]
    [SerializeField]
    private string itemName = "New Item";
    [SerializeField]
    private int itemID = 0;
    [SerializeField]
    private ItemType placementType = ItemType.Floor;
    //[SerializeField]
    //private int maxCount = 5; // 최대 배치 가능 갯수

    [Header("Price Info")]
    [SerializeField]
    private int price = 10;

    [Header("Runtime Data")]
    [SerializeField]
    private bool isPlaced = false;
    [SerializeField]
    private Vector2Int gridPosition;



    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            sprite = spriteRenderer.sprite;
            itemColor = spriteRenderer.color;
        }
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

    public string GetItemName()
    {
        return itemName;
    }

    public int GetItemID()
    {
        return itemID;
    }

    public Sprite GetSprite()
    {
        if (sprite != null) return sprite;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer != null ? spriteRenderer.sprite : null;
    }

    public GameObject GetItemPrefab()
    {
        return this.gameObject; // 편집모드용 프리팹
    }

    public ItemType GetPlacementType()
    {
        return placementType;
    }

    public Color GetItemColor()
    {
        if (spriteRenderer == null) // SpriteRenderer 캐시 없으면 새로 가져오기
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) // SpriteRenderer가 있으면 거기서 색상 읽기
        {
            Color c = spriteRenderer.color;

            
            if (c.a <= 0.01f || (c.r == 0 && c.g == 0 && c.b == 0)) // 만약 완전 투명하거나 검정이면 기본 white로 보정
                return Color.white;

            return c;
        }
        
        if (itemColor.a <= 0.01f || (itemColor.r == 0 && itemColor.g == 0 && itemColor.b == 0)) // spriteRenderer가 없으면 itemColor 값 사용
            return Color.white; // (만약 기본값이면 white로 보정)

        return itemColor;
    }

    public bool GetIsPlaced()
    {
        return isPlaced;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    public int GetPrice()
    {
        return price;
    }
}
