using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* ====== 싱글톤 변수 ====== */
    public static GameManager instance = null; // 싱글톤 변수

    /* ====== 재화 변수 ====== */
    [Header("재화")]
    [SerializeField]
    private int coin = 0;
    [SerializeField]
    private int specialCoin = 0;

    [Header("아이템")]
    [SerializeField] private List<RoomDecoItem> itemList = new List<RoomDecoItem>();

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
        /* 초기 재화 설정 */
        coin = 0;
        specialCoin = 0;
    }

    /* ====== 재화 관련 함수 ====== */
    public int GetCoin() // 일반 코인 가져오기
    {
        return coin;
    }

    public void AddCoin(int amount) // 일반 코인 추가
    {
        coin += amount;
    }

    public void SubtractCoin(int amount) // 일반 코인 차감
    {
        coin -= amount;
        if (coin < 0)
            coin = 0;
    }

    public int GetSpecialCoin() // 스페셜 코인 가져오기
    {
        return specialCoin;
    }

    public void AddSpecialcoin(int amount) // 스페셜 코인 추가
    {
        specialCoin += amount;
    }

    public void SubtractSpecialCoin(int amount) // 스페셜 코인 차감
    {
        specialCoin -= amount;
        if (specialCoin < 0)
            specialCoin = 0;
    }

    
}
