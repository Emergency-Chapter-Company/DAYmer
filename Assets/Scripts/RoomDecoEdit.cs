using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomDecoEdit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button editModeButton;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject editModeUI;      // 취소/확인 버튼 포함하기

    [Header("Edit Mode UI")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [Header("Room Management")]
    [SerializeField] private Transform roomContainer; //아이템들이 배치될 부모 오브젝트

    private RoomDecoState currentState = new RoomDecoState();
    private RoomDecoState backupState = null;
    private List<GameObject> currentPlacedObjects = new List<GameObject>();

    private bool isEditMode = false;

    private void Start()
    {
        editModeButton.onClick.AddListener(EnterEditMode);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);

        // 초기 상태 설정 : 편집 모드 비활성화
        inventoryPanel.SetActive(false);
        editModeUI.SetActive(false);
    }

    public void EnterEditMode()
    {
        isEditMode = true;

        // 현재 상태 백업
        BackupCurrentState();


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

    private void BackupCurrentState()
    {
        currentState.placedItems.Clear();

        foreach (Transform child in roomContainer)
        {
            PlacedItemData data = new PlacedItemData(
                null,
                child.position,
                child.rotation
                );
            currentState.placedItems.Add(data);
        }
        backupState = currentState.clone();
        Debug.Log($"백업 : {backupState.placedItems.Count}개 아이템");
    }

    private void RestoreBackupState()
    {
        // 현재 배치된 아이템 제거
        foreach (GameObject obj in currentPlacedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentPlacedObjects.Clear();

        // 백업된 상태로 복원
        if (backupState != null)
        {
            foreach (PlacedItemData data in backupState.placedItems)
            {
                if (data.itemPrefab != null)
                {
                    GameObject restored = Instantiate(data.itemPrefab, roomContainer);
                    restored.transform.position = data.position;
                    restored.transform.rotation = data.rotation;
                    currentPlacedObjects.Add(restored);
                }

            }
            Debug.Log($"복원 : {backupState.placedItems.Count}개 아이템");
        }
    }

    private void ApplyCurrentState()
    {
        //현재 배치상태 확정
        currentState.placedItems.Clear();

        foreach (GameObject obj in currentPlacedObjects)
        {
            if (obj != null)
            {
                PlacedItemData data = new PlacedItemData(
                   null,
                   obj.transform.position,
                   obj.transform.rotation
                );                   
                currentState.placedItems.Add(data);
            }
        }
        Debug.Log($"상태 적용 : {currentState.placedItems.Count}개 아이템");
    }

    private void OnCancelButtonClicked()
    {
        Debug.Log("편집 모드 취소 버튼이 클릭되었습니다.");
        // 이전 상태로 상대 복원
        RestoreBackupState();
        ExitEditMode();
    }


    private void OnConfirmButtonClicked()
    {
        Debug.Log("편집 모드 확인 버튼이 클릭되었습니다.");
        // 현재상태 저장 기능 추가예정
        ApplyCurrentState();
        ExitEditMode();
    }

    // 외부에서 아이템 추가 시 호출
    public void AddPlacedItem(GameObject item)
    {
        currentPlacedObjects.Add(item);
    }

    public Transform GetRoomContainer()
    {
        return roomContainer;
    }

    public void ResisterPlacedItem(GameObject item)
    {
        currentPlacedObjects.Add(item);
    }
}
