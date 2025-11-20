using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndividualSceneUI : MonoBehaviour
{
    /* ====== 객체 변수 ====== */
    private GameManager gameManager;

    /* ====== UI 변수 ====== */
    [Header("UI")]
    [SerializeField]
    private TMP_Text coinText;
    [SerializeField]
    private Button sceneChangeButton;

    private void Awake()
    {
        gameManager = GameManager.instance; // (시도 1/3)

        if (coinText == null)
            Debug.LogWarning("[IndividualSceneUI] ⚠ coinText가 연결되지 않음");
        if (sceneChangeButton == null)
            Debug.LogWarning("[IndividualSceneUI] ⚠ sceneChangeButton이 연결되지 않음");

        if (gameManager == null)
            gameManager = GameManager.instance; // (시도 2/3)

        if (gameManager != null)
            gameManager.RegisterIndividualSceneUIs(coinText, sceneChangeButton);
        else
            Debug.LogWarning("[IndividualSceneUI(Awake)] GameManager 인스턴스 없음 (시도 2/3)");
    }

    private void Start()
    {
        if (gameManager == null)
            gameManager = GameManager.instance; // (시도 3/3)

        if (gameManager != null)
            gameManager.RegisterIndividualSceneUIs(coinText, sceneChangeButton);
        else
            Debug.LogError("[IndividualSceneUI(Start)] GameManager 인스턴스 없음 (시도 3/3)");
    }
}
