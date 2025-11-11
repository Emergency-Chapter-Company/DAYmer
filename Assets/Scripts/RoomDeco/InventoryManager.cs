using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Inventory Items")]
    [SerializeField] private List<RoomDecoItem> itemList = new List<RoomDecoItem>();

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
            nameText.text = item.GetItemName();

            // 드래그 기능
            DraggableItem draggable = slot.AddComponent<DraggableItem>();
            draggable.SetItemData(item);
        }
    }

}
