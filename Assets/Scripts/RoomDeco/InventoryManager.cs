using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    /* ====== 객체 변수 ====== */
    private GameManager gameManager;

    /* ====== UI 컴포넌트 ====== */
    [Header("UI References")]
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject itemSlotPrefab;

    /* ====== 아이템 리스트 ====== */
    [Header("Inventory Items")]
    [SerializeField] private List<RoomDecoItem> itemList = new List<RoomDecoItem>();

    private void Awake()
    {
        gameManager = GameManager.instance;

        if (gameManager != null)
            gameManager.RegisterInventoryManager(this);
    }

    private void Start()
    {
        PopulateInventory();
    }

    private void PopulateInventory()
    {
        // 기존 슬롯 삭제
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        // 아이템 슬롯 생성
        foreach (RoomDecoItem item in itemList)
        {
            GameObject slot = Instantiate(itemSlotPrefab, inventoryContent);

            // 슬롯 정보 지정
            Image icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI nameText = slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();

            icon.sprite = item.GetSprite();
            icon.color = item.GetItemColor();
            nameText.text = item.GetItemName();

            // 드래그 기능
            DraggableItem draggable = slot.AddComponent<DraggableItem>();
            draggable.SetItemData(item);
        }
    }

    public void AddItem(RoomDecoItem newItem)
    {
        if (!itemList.Contains(newItem))
        {
            itemList.Add(newItem);
            Debug.Log($"[InventoryManager] {newItem.GetItemName()} 추가됨");

            PopulateInventory(); // UI 갱신
        }
        else
        {
            Debug.Log($"[InventoryManager] {newItem.GetItemName()} 이미 존재함");
        }
    }

    public bool HasItem(RoomDecoItem item)
    {
        return itemList.Contains(item);
    }
}
