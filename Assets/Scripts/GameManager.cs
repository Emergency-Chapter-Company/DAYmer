using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* ====== 싱글톤 변수 ====== */
    public static GameManager instance = null; // 싱글톤 변수

    /* ====== 재화 변수 ====== */
    [Header("재화")]
    [SerializeField]
    private int normalCoin = 0;
    [SerializeField]
    private int specialCoin = 0;

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
        normalCoin = 0;
        specialCoin = 0;
    }

    /* ====== 재화 관련 함수 ====== */
    public int Getcoin() // 일반 코인 가져오기
    {
        return normalCoin;
    }

    public void Addcoin(int amount) // 일반 코인 추가
    {
        normalCoin += amount;
    }

    public void Subtractcoin(int amount) // 일반 코인 차감
    {
        normalCoin -= amount;
        if (normalCoin < 0)
            normalCoin = 0;
    }

    public int GetDiamondcoin() // 다이아몬드 코인 가져오기
    {
        return specialCoin;
    }

    public void AddDiamondcoin(int amount) // 다이아몬드 코인 추가
    {
        specialCoin += amount;
    }

    public void SubtractDiamondCoin(int amount) // 다이아몬드 코인 차감
    {
        specialCoin -= amount;
        if (specialCoin < 0)
            specialCoin = 0;
    }
}
