using UnityEngine;

// 아이템 배치 시스템

public class RoomDecoPlace : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomDecoGrid gridSystem;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private string floorSortingLayer = "Floor";

    private RoomDecoItem currentItem;
    private ItemData2D currentItemData;
    private bool isPlacing = false;

    private RoomDecoCore Manager;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (gridSystem == null)
            gridSystem = FindObjectOfType<RoomDecoGrid>();

        Manager = FindObjectOfType<RoomDecoCore>();
    }

    private void Update()
    {
        if (isPlacing && currentItem != null)
        {
            UpdateItemPosition();
            HandleInput();
        }
    }

    public void StartPlacing(ItemData2D itemData)
    {
        if (itemData == null || itemData.Sprite == null)
        {
            Debug.LogWarning("ItemData 또는 Sprite가 Null입니다.");
            return;
        }

        //아이템 제거
        if (currentItem != null && currentItem.IsPlaced())
        {
            Destroy(currentItem.gameObject);
        }

        // 새 아이템 생성
        GameObject itemObj = new GameObject(itemData.ItemName);

        // 스프라이트 렌더러 추가
        SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();
        sr.sprite = itemData.Sprite;
        sr.sortingLayerName = floorSortingLayer;

        // RoomDecoItem 추가
        currentItem = itemObj.AddComponent<RoomDecoItem>();
        currentItem.SetItemData(itemData);
        currentItemData = itemData;

        currentItem.SetPlaced(false);
        isPlacing = true;

        Debug.Log($"아이템 배치 시작: {itemData.ItemName}");
    }

    private void UpdateItemPosition()
    {
        // 마우스 위치
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // 그리드 좌표 변환
        Vector2Int gridPos = gridSystem.WorldToGrid(mouseWorldPos);

        // 그리드 범위 확인
        if (gridSystem.IsWithinGrid(gridPos))
        {
            Vector2 worldPos = gridSystem.GridToWorld(gridPos);
            currentItem.transform.position = worldPos;
            currentItem.SetGridPosition(gridPos);
        }
    }

    private void HandleInput()
    {
        // 좌클릭 : 배치
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceItem();
        }

        // 우클릭 : 취소
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacing();
        }
    }

    private void TryPlaceItem()
    {
        if (currentItem == null)
            return;

        currentItem.SetPlaced(true);

        int sortingOrder = -(int)(currentItem.transform.position.y * 100);
        currentItem.SetSortingOrder(sortingOrder);

        // 배치결과 전달
        if (Manager != null)
        {
            Manager.OnItemPlaced(currentItem);
        }

        Debug.Log($"아이템 배치 완료: {currentItemData.ItemName} at {currentItem.GetGridPosition()}");

        //배치 모드 종료
        currentItem = null;
        isPlacing = false;
        currentItemData = null;
    }

    private void CancelPlacing()
    {
        if (currentItem != null && !currentItem.IsPlaced())
        {
            Destroy(currentItem.gameObject);
        }

        currentItem = null;
        isPlacing = false;
        currentItem = null;

        Debug.Log("아이템 배치 취소");
    }

    public bool IsPlacing() => isPlacing;
}

        