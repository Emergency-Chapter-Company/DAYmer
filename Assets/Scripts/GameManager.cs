using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* ====== 싱글톤 변수 ====== */
    public static GameManager instance = null; // 싱글톤 변수

    /* ====== 컴포넌트 ====== */
    private SaveController saveController;

    /* ====== 재화 변수 ====== */
    [Header("재화")]
    [SerializeField]
    private int coin = 0;
    [SerializeField]
    private int specialCoin = 0;

    [Header("컴포넌트")]
    [SerializeField]
    private InventoryManager inventoryManager;
    [SerializeField]
    private TimerManager timerManager;

    void Awake()
    {
        // 싱글톤 설정
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
    }

    void Start()
    {
        saveController = GetComponent<SaveController>();

        LoadGame();
    }

    public void LoadGame()
    {
        if (saveController == null)
        {
            Debug.LogWarning("SaveController가 없습니다!");
            return;
        }

        SaveData data = saveController.Load();

        /* 저장된 데이터 반영 */
        coin = data.coin;
        specialCoin = data.specialCoin;

        if (timerManager != null)
            timerManager.LoadRecords(data.savedRecords);

        Debug.Log("게임 데이터 로드 완료");
    }

    public void SaveGame()
    {
        if (saveController == null)
            return;

        SaveData data = new SaveData();

        // 현재 상태 저장
        data.coin = coin;
        data.specialCoin = specialCoin;

        // 시간 기록 저장
        if (timerManager != null)
        {
            data.savedRecords = timerManager.GetRecordData();
        }

        saveController.Save(data);
    }

    /* ====== 외부 호출 함수 ====== */
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

    public void AddItemToInventory(RoomDecoItem newItem)
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager 참조가 없습니다!");
            return;
        }

        inventoryManager.AddItem(newItem);
        Debug.Log($"[GameManager] 인벤토리에 {newItem.GetItemName()} 추가됨");
    }
}
