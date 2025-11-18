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
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        saveController = GetComponent<SaveController>();
    }

    private void Start()
    {
        if (LoadedData == null)
            LoadGame();
    }

    /* ====== 매니저 등록 ====== */
    public void RegisterTimerManager(TimerManager manager)
    {
        timerManager = manager;
        TryApplyLoadedData();
    }

    public void RegisterInventoryManager(InventoryManager manager)
    {
        inventoryManager = manager;
        TryApplyLoadedData();
    }

    public void RegisterRoomDecoEdit(RoomDecoEdit edit)
    {
        roomDecoEdit = edit;
        TryApplyLoadedData();
    }

    /* ====== 저장/로드 관련 ====== */
    public void LoadGame()
    {
        if (saveController == null)
        {
            Debug.LogWarning("[GameManager] SaveController가 없음");
            return;
        }

        // 세이브 파일에서 항상 최신 데이터 읽어옴
        LoadedData = saveController.Load();

        // 저장된 데이터 반영
        coin = LoadedData.coin;
        specialCoin = LoadedData.specialCoin;

        // 현재 씬에 존재하는 매니저들에게 데이터 적용
        TryApplyLoadedData();

        Debug.Log("[GameManager] 게임 데이터 로드 완료");
        Debug.Log($"[GameManager] 코인: {coin}, 스페셜 코인: {specialCoin}");
    }

    public void SaveGame()
    {
        if (saveController == null)
            return;

        // 기존 데이터 불러오기 (없으면 새로 생성됨)
        SaveData data = saveController.Load();

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

        // 메모리에도 최신 상태 유지
        LoadedData = data;
    }

    private void TryApplyLoadedData()
    {
        // 저장 데이터가 없으면 아무것도 안 함
        if (LoadedData == null) return;

        // 타이머
        if (timerManager != null)
            timerManager.LoadRecords(LoadedData.savedTimeRecords);

        // 인벤토리
        if (inventoryManager != null)
            inventoryManager.RestoreInventory(LoadedData.ownedItemIDs);

        // 방 꾸미기
        if (roomDecoEdit != null)
            roomDecoEdit.LoadRoomState(LoadedData.placedItems);
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
