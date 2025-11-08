using UnityEngine;
using UnityEngine.UI;

public class RoomDecoEdit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button editModeButton;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject editModeUI;      // 취소/확인 버튼 포함하기

    private bool isEditMode = false;

    private void Start()
    {
        editModeButton.onClick.AddListener(EnterEditMode);

        // 초기 상태 설정 : 편집 모드 비활성화
        inventoryPanel.SetActive(false);
        editModeUI.SetActive(false);
    }

    public void EnterEditMode()
    {
        isEditMode = true;

        // UI 활성화
        editModeButton.gameObject.SetActive(false); // 편집 버튼 숨기기
        inventoryPanel.SetActive(true);             // 인벤토리 패널 표시
        editModeUI.SetActive(true);                 // 편집 모드 UI 표시

        Debug.Log("편집 모드에 진입했습니다.");
    }

    public void ExitEditMode()
    {
        isEditMode = false;

        // UI 비활성화
        editModeButton.gameObject.SetActive(true);  // 편집 버튼 표시
        inventoryPanel.SetActive(false);            // 인벤토리 패널 숨기기
        editModeUI.SetActive(false);                // 편집 모드 UI 숨기기

        Debug.Log("편집 모드에서 나갔습니다.");
    }

    public bool IsEditMode()
    {
        return isEditMode;
    }
}
