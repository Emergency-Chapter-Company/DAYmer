using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* ====== 싱글톤 변수 ====== */
    public static GameManager instance = null; // 싱글톤 변수

    /* ====== 컴포넌트 ====== */
    private SaveController saveController;
    private SaveData LoadedData;

    private InventoryManager inventoryManager;
    private TimerManager timerManager;
    private RoomDecoEdit roomDecoEdit;

    /* ====== 재화 변수 ====== */
    [Header("재화")]
    [SerializeField]
    private int coin = 0;
    [SerializeField]
    private int specialCoin = 0;

    /* ====== 유니티 생명주기 ====== */
    private void Awake()
    {
        /* 싱글톤 설정 */
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(GameManager.instance.gameObject);

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        saveController = GetComponent<SaveController>();
    }

    /* ====== 매니저 등록 ====== */
    public void RegisterTimerManager(TimerManager manager)
    {
        timerManager = manager;
    }

    public void RegisterInventoryManager(InventoryManager manager)
    {
        inventoryManager = manager;
    }

    public void RegisterRoomDecoEdit(RoomDecoEdit edit)
    {
        roomDecoEdit = edit;
    }

    /* ====== 저장/로드 관련 ====== */
    public void LoadGame()
    {
        if (saveController == null)
        {
            Debug.LogWarning("[GameManager] SaveController가 없음");
            return;
        }

        LoadedData = saveController.Load();

        /* 저장된 데이터 반영 */
        coin = LoadedData.coin;
        specialCoin = LoadedData.specialCoin;

        if (timerManager != null)
            timerManager.LoadRecords(LoadedData.savedTimeRecords);

        if (inventoryManager != null)
            inventoryManager.RestoreInventory(LoadedData.ownedItemIDs);

        if (roomDecoEdit != null)
            roomDecoEdit.LoadRoomState(LoadedData.placedItems);

        Debug.Log("[GameManager] 게임 데이터 로드 완료");
        Debug.Log($"[GameManager] 코인: {coin}, 스페셜 코인: {specialCoin}");
    }

    public void SaveGame()
    {
        if (saveController == null)
            return;

        SaveData data = new SaveData();

        // 현재 상태 저장
        data.coin = coin;
        data.specialCoin = specialCoin;
        
        if (timerManager != null) // 시간 기록 저장
            data.savedTimeRecords = timerManager.GetRecordData();

        if (inventoryManager != null) // 인벤토리 저장
            data.ownedItemIDs = inventoryManager.GetOwnedItemIDs();

        if (roomDecoEdit != null) // 배치된 아이템 저장
            data.placedItems = roomDecoEdit.GetRoomStateForSave();

        saveController.Save(data);
    }

    /* ====== 재화 관련 ====== */
    public int GetCoin() // 일반 코인 가져오기
    {
        return coin;
    }

    public void AddCoin(int amount) // 일반 코인 추가
    {
        coin += amount;
        SaveGame();
    }

    public void SubtractCoin(int amount) // 일반 코인 차감
    {
        coin -= amount;
        if (coin < 0)
            coin = 0;
        SaveGame();
    }

    public int GetSpecialCoin() // 스페셜 코인 가져오기
    {
        return specialCoin;
    }

    public void AddSpecialcoin(int amount) // 스페셜 코인 추가
    {
        specialCoin += amount;
        SaveGame();
    }

    public void SubtractSpecialCoin(int amount) // 스페셜 코인 차감
    {
        specialCoin -= amount;
        if (specialCoin < 0)
            specialCoin = 0;
        SaveGame();
    }

    /* ====== 인벤토리 관련 ====== */
    public void AddItemToInventory(RoomDecoItem newItem)
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("[GameManger] InventoryManager 참조가 없음");
            return;
        }

        inventoryManager.AddItem(newItem);
        SaveGame();
        Debug.Log($"[GameManager] 인벤토리에 {newItem.GetItemName()} 추가됨");
    }
}
