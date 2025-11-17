using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    /* ====== 객체 변수 ====== */
    private GameManager gameManager;
    private InventoryManager inventoryManager;

    [Header("UI References")]
    [SerializeField] private Button storeButton;
    [SerializeField] private Button editModeButton;
    [SerializeField] private GameObject storePanel;
    [SerializeField] private Button exitButton;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab; // UI 슬롯 프리팹

    [Header("Store Item Prefabs")]
    [SerializeField] private List<RoomDecoItem> storeItems = new List<RoomDecoItem>();

    private void Start()
    {
        storeButton.onClick.AddListener(OpenStore);
        exitButton.onClick.AddListener(CloseStore);

        gameManager = GameManager.instance;
        inventoryManager = GetComponent<InventoryManager>();

        gameManager.RegisterInventoryManager(inventoryManager);

        storePanel.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }

    private void OpenStore()
    {
        storeButton.gameObject.SetActive(false);
        editModeButton.gameObject.SetActive(false);
        storePanel.SetActive(true);
        exitButton.gameObject.SetActive(true);
        RefreshStore();
    }

    private void CloseStore()
    {
        storeButton.gameObject.SetActive(true);
        editModeButton.gameObject.SetActive(true);
        storePanel.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }

    private void RefreshStore()
    {
        // 기존 슬롯 제거
        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        // 슬롯 새로 생성
        foreach (RoomDecoItem item in storeItems)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);

            Image icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI nameText = slot.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
            Button buyButton = slot.transform.Find("BuyButton").GetComponent<Button>();
            TextMeshProUGUI priceText = slot.transform.Find("BuyButton/PriceText").GetComponent<TextMeshProUGUI>();

            // 구매 상태 체크
            bool isOwned = inventoryManager != null && inventoryManager.HasItem(item);
            bool isAffordable = gameManager.GetCoin() >= item.GetPrice();

            icon.sprite = item.GetSprite();
            icon.color = item.GetItemColor();
            nameText.text = item.GetItemName();

            if (isOwned) // 이미 구매한 아이템
            {
                buyButton.interactable = false;
                priceText.text = "Sold Out";
                priceText.color = Color.gray;
            }
            else
            {
                priceText.text = $"{item.GetPrice()} Coin";
                priceText.color = isAffordable ? Color.black : Color.red;

                buyButton.interactable = true;
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => TryBuyItem(item, priceText, buyButton));
            }
        }
    }

    private void TryBuyItem(RoomDecoItem item, TextMeshProUGUI priceText, Button buyButton)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager가 없습니다.");
            return;
        }

        int price = item.GetPrice();

        if (gameManager.GetCoin() >= price)
        {
            gameManager.SubtractCoin(price);
            gameManager.AddItemToInventory(item);

            Debug.Log($"{item.GetItemName()} 구매 성공! 남은 코인: {gameManager.GetCoin()}");

            // UI 갱신
            priceText.text = "Sold Out";
            priceText.color = Color.gray;
            buyButton.interactable = false;
        }
        else
        {
            Debug.Log("코인이 부족합니다!");
        }
    }

    public RoomDecoItem GetItemByID(int id)
    {
        return storeItems.Find(item => item.GetItemID() == id);
    }
}
