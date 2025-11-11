using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    /* ====== 객체 변수 ====== */
    private GameManager gameManager;

    [Header("UI References")]
    [SerializeField] private Button storeButton;
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

        storePanel.SetActive(false);
        exitButton.gameObject.SetActive(false);
    }

    private void OpenStore()
    {
        storePanel.SetActive(true);
        exitButton.gameObject.SetActive(true);
        RefreshStore();
    }

    private void CloseStore()
    {
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

            icon.sprite = item.GetSprite();
            icon.color = item.GetItemColor();
            nameText.text = item.GetItemName();
            priceText.text = $"{item.GetPrice()} Coin";

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => TryBuyItem(item));
        }
    }

    private void TryBuyItem(RoomDecoItem item)
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
            Debug.Log($"{item.GetItemName()} 구매 성공! 남은 코인: {gameManager.GetCoin()}");

            // TODO: 인벤토리에 추가하는 로직이 있다면 여기서 호출
            // InventoryManager.Instance.AddItem(item);
        }
        else
        {
            Debug.Log("코인이 부족합니다!");
        }
    }
}
