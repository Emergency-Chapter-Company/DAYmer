using UnityEngine;
using System.Collections.Generic;


// 아이템 타입 정의
public enum ItemType
{
    Floor,
    Wall,
    Ceiling
}

public class RoomDecoCore : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private RoomDecoGrid gridSystem;
    [SerializeField] private RoomDecoPlace itemPlacer;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Test Items")]
    [SerializeField] private List<ItemData2D> testItems = new List<ItemData2D>();

    private List<RoomDecoItem> placedItems = new List<RoomDecoItem>();

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (gridSystem == null)
            gridSystem = FindObjectOfType<RoomDecoGrid>();

        if (itemPlacer == null)
            itemPlacer = FindObjectOfType<RoomDecoPlace>();

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
}