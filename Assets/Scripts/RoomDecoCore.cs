using UnityEngine;
using System.Collections.Generic;


// 아이템 타입 정의
public enum ItemType
{
    Floor,      // 바닥
    LeftWall,   // 왼쪽 벽
    RightWall   // 오른쪽 벽
}

public enum WallDirection
{
    North,
    South,
    East,
    West
}

public class RoomDecoCore : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private RoomDecoPlace itemPlacer;

    [Header("Grid System")]
    [SerializeField] private RoomDecoGrid_Floor floorGrid;
    [SerializeField] private RoomDecoGrid_LeftWall leftWallGrid;
    [SerializeField] private RoomDecoGrid_RightWall rightWallGrid;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Test Items")]
    [SerializeField] private List<ItemData2D> testItems = new List<ItemData2D>();

    private List<RoomDecoItem> placedItems = new List<RoomDecoItem>();

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (itemPlacer == null)
            itemPlacer = FindObjectOfType<RoomDecoPlace>();

        if (floorGrid == null)
            floorGrid = FindObjectOfType<RoomDecoGrid_Floor>();

        if (leftWallGrid == null)
            leftWallGrid = FindObjectOfType<RoomDecoGrid_LeftWall>();

        if (rightWallGrid == null)
            rightWallGrid = FindObjectOfType<RoomDecoGrid_RightWall>();

    }

    private void Update()
    {
        //테스트용 키
        for (int i = 0; i < testItems.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                StartPlacingItem(testItems[i]);
            }
        }
    }

    public void StartPlacingItem(ItemData2D itemData)
    {
        if (itemPlacer != null && itemData != null)
        {
            itemPlacer.StartPlacing(itemData);
        }
    }

    public void OnItemPlaced(RoomDecoItem item)
    {
        if (item != null && !placedItems.Contains(item))
        {
            placedItems.Add(item);
            Debug.Log($"아이템 배치: {item.GetItemName()}");
        }
    }

    // 아이템 타입에 따라 적절한 그리드 반환

    public RoomDecoGrid GetGridByItemType(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Floor:
                return floorGrid;
            case ItemType.LeftWall:
                return leftWallGrid;
            case ItemType.RightWall:
                return rightWallGrid;
            default:
                return floorGrid;
        }
    }
}