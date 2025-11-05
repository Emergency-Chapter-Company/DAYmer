using UnityEngine;

// 아이템 배치 시스템

public class RoomDecoPlace : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomDecoCore coreManager;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private string floorSortingLayer = "Floor";

    // 영역 구분 설정
    [Header("Area Boundaries")]
    [SerializeField] private float leftWallMinY = 0f;   // 왼쪽 벽 최소 Y좌표
    [SerializeField] private float leftWallMaxX = 5f;   // 왼쪽 벽 최대 X좌표
    [SerializeField] private float rightWallMinY = 0f;  // 오른쪽 벽 최소 Y좌표
    [SerializeField] private float rightWallMinX = 5f;   // 오른쪽 벽 최소 X좌표

    private RoomDecoItem currentItem;
    private ItemData2D currentItemData;
    private bool isPlacing = false;
    private RoomDecoGrid currentGrid;

    private RoomDecoCore Manager;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if(coreManager == null)
           coreManager = FindObjectOfType<RoomDecoCore>();
    }

    private void Update()
    {
        if (isPlacing && currentItem != null)
        {
            UpdateItemPosition();
            HandleInput();
        }
    }

    // 아이템 배치를 위한 기본 설정
    public void StartPlacing(ItemData2D itemData)
    {
        if (itemData == null || itemData.Sprite == null)
        {
            Debug.LogWarning("ItemData 또는 Sprite가 Null입니다.");
            return;
        }

        // 기존 아이템 제거
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
        }

        // 아이템 타입에 맞는 그리드 선택
        currentGrid = coreManager.GetGridByItemType(itemData.PlacementType);

        if (currentGrid == null)
        {
            Debug.LogWarning("해당 타입에 맞는 그리드를 찾을수 없습니다 : {itemData.PlacementType}");
            return;
        }

        // 새 아이템 생성
        GameObject itemObj = new GameObject(itemData.ItemName);


        // 스프라이트 렌더러 추가
        SpriteRenderer sr = itemObj.AddComponent<SpriteRenderer>();
        sr.sprite = itemData.Sprite;
        sr.color = itemData.ItemColor;
        sr.sortingLayerName = floorSortingLayer;


        // RoomDecoItem 추가
        currentItem = itemObj.AddComponent<RoomDecoItem>();
        currentItem.SetItemData(itemData);
        currentItemData = itemData;

        currentItem.SetPlaced(false);
        isPlacing = true;

        Debug.Log($"아이템 배치 시작: {itemData.ItemName}, 그리드 : {currentGrid.GetType().Name}");
    }

    private void UpdateItemPosition()
    {
        
        
        if (currentItem != null)
            return;
         

        // 마우스 위치 
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log($"마우스 월드 좌표: {mouseWorldPos}");

        // 마우스 현재 위치의 영역 확인
        ItemType currentArea = GetAreaType(mouseWorldPos);

        // 현재 아이템 타입과 영역이 맞는지 확인
        Vector2Int gridPos = currentGrid.WorldToGrid(mouseWorldPos);
        Debug.Log($"그리드 좌표: {gridPos}, IsWithinGrid: {currentGrid.IsWithinGrid(gridPos)}");

        if (currentGrid.IsWithinGrid(gridPos))
        {
            Vector2 worldPos = currentGrid.WorldToGrid(mouseWorldPos);
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
        if (currentItem == null || currentGrid == null)
            return;

        Vector2Int gridPos = currentItem.GetGridPosition();
        Vector2 worldPos = currentItem.transform.position;

        // 올바른 영역-아이템 체크
        ItemType currentArea = GetAreaType(worldPos);
        if (currentArea != currentItemData.PlacementType)
        {
            Debug.Log($"아이템을 올바른 영역에 배치해야 합니다. 현재 영역: {currentArea}, 아이템 타입: {currentItemData.PlacementType}");
            return;
        }

        Debug.Log($"배치 시도 위치: {gridPos}, 타입 : {currentItemData.PlacementType}");

        // 중복 배치 체크
        if (currentGrid.IsTileOccupied(gridPos, currentItemData.PlacementType))
        {
            Debug.Log("해당 위치에 이미 아이템이 배치되어 있습니다.");
            return;
        }



        currentItem.SetPlaced(true);


        // 배치 그리드 등록
        currentGrid.OccupyTile(gridPos, currentItemData.PlacementType);

        // 레이어 순서 (보이기 우선순위 : 천장 > 바닥 > 벽)

        int sortingOrder = GetSortingOrder(currentItemData.PlacementType, currentItem.transform.position.y);
        currentItem.SetSortingOrder(sortingOrder);

        // 배치결과 전달
        if (Manager != null)
        {
            Manager.OnItemPlaced(currentItem);
        }

        Debug.Log($"아이템 배치 완료: {currentItemData.ItemName} at {gridPos}");

        //배치 모드 종료
        currentItem = null;
        isPlacing = false;
        currentItemData = null;
        currentItem = null;
    }

    private int GetSortingOrder(ItemType type, float yPos)
    {
        int baseOrder = -(int)(yPos * 100);

        switch (type)
        {
            case ItemType.Floor:
                return baseOrder + 200; // 천장 맨 위
            case ItemType.LeftWall:
                return baseOrder + 100; // 바닥 중간
            case ItemType.RightWall:
                return baseOrder;       // 벽 맨 아래
            default:
                return baseOrder;
        }
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

    private ItemType GetAreaType(Vector2 worldPos)
    {

        // Y좌표와 X좌표로 영역 구분
        // 왼쪽 위 = LeftWall
        // 중앙 = Floor
        // 오른쪽 아래 = RightWall


        // 왼쪽 벽 영역
        if (worldPos.y >= leftWallMinY && worldPos.x <= leftWallMaxX)
            return ItemType.LeftWall;

        // 오른쪽 벽 영역
        else if (worldPos.y <= rightWallMinY && worldPos.x >= rightWallMinX)
            return ItemType.RightWall;

        // 바닥 영역
        else
            return ItemType.Floor;
    }


    public bool IsPlacing() => isPlacing;
}

        