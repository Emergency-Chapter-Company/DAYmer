using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    /* ====== 객체 변수 ====== */
    private GameManager gameManager;
    private StoreManager storeManager;

    /* ====== 상태 변수 ====== */
    private bool inventoryRestored = false;

    /* ====== UI 컴포넌트 ====== */
    [Header("UI References")]
    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject itemSlotPrefab;

    /* ====== 아이템 리스트 ====== */
    [Header("Inventory Items")]
    [SerializeField] private List<RoomDecoItem> ownedItemList = new List<RoomDecoItem>();

    private void Awake()
    {
        gameManager = GameManager.instance;
    }

    private void Start()
    {
        if (gameManager == null)
            gameManager = GameManager.instance; // 두 번째 안전 체크

        if (gameManager != null)
            gameManager.RegisterInventoryManager(this);
        else
            Debug.LogError("[InventoryManager] GameManager 인스턴스 없음");

        storeManager = GetComponent<StoreManager>();

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
        foreach (RoomDecoItem item in ownedItemList)
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
        if (!ownedItemList.Contains(newItem))
        {
            ownedItemList.Add(newItem);
            Debug.Log($"[InventoryManager] {newItem.GetItemName()} 추가됨");

            PopulateInventory(); // UI 갱신
            gameManager.SaveGame();
        }
        else
        {
            Debug.Log($"[InventoryManager] {newItem.GetItemName()} 이미 존재함");
        }
    }

    public bool HasItem(RoomDecoItem item)
    {
        return ownedItemList.Contains(item);
    }

    public List<int> GetOwnedItemIDs()
    {
        List<int> result = new List<int>();
        foreach (var item in ownedItemList)
            result.Add(item.GetItemID());
        return result;
    }

    public void RestoreInventory(List<int> savedIDs)
    {
        if (inventoryRestored) return;

        StartCoroutine(RestoreDelayed(savedIDs));
    }

    private IEnumerator RestoreDelayed(List<int> savedIDs)
    {
        // StoreManager, UI가 완전히 준비될때까지 기다림
        yield return null;

        if (storeManager == null)
            storeManager = GetComponent<StoreManager>();

        ownedItemList.Clear();

        foreach (var id in savedIDs)
        {
            RoomDecoItem match = storeManager != null ? storeManager.GetItemByID(id) : null;
            if (match != null)
                ownedItemList.Add(match);
            else
                Debug.LogWarning($"[InventoryManager] 저장된 ID({id}) 아이템을 찾을 수 없음");
        }

        PopulateInventory();

        inventoryRestored = true;
        Debug.Log("[InventoryManager] 인벤토리 로드 완료");
    }
}
