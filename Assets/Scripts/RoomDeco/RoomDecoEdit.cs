using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private Button resetButton;
    [SerializeField] private Button deleteButton;

    [Header("Room Management")]
    [SerializeField] private Transform roomContainer; //아이템들이 배치될 부모 오브젝트

    private RoomDecoState currentState = new RoomDecoState();
    private RoomDecoState backupState = null;
    private List<GameObject> currentPlacedItemList = new List<GameObject>();

    // 아이템 선택
    private GameObject selectedItem = null;
    private SpriteRenderer selectedItemRenderer = null;
    private Color originalColor;

    private bool isEditMode = false;

    // 인벤토리 매니저 참조
    private InventoryManager inventoryManager;

    private void Start()
    {
        /* 버튼 리스너 등록 */
        editModeButton.onClick.AddListener(EnterEditMode);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        resetButton.onClick.AddListener(OnResetButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);

        // 초기 상태 설정 : 편집 모드 비활성화
        inventoryPanel.SetActive(false);
        editModeUI.SetActive(false);
        deleteButton.interactable = false;   // 삭제버튼 초기 비활성화
    }

    private void Update()
    {
        if (!isEditMode)
            return;

        // 빈 공간 클릭 시 선택 해제
        if (isEditMode && Input.GetMouseButtonDown(0))
        {
            // UI 클릭이면 무시
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            Debug.Log($"Raycast2D Hit : {hit.collider}");

            if (hit.collider == null)
            {
                DeselectItem();
            }
        }
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
        DeselectItem();         // 선택해제

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
            RoomDecoItem itemComponent = child.GetComponent<RoomDecoItem>();
            GameObject prefab = null;

            if (itemComponent != null)
            {
                prefab = itemComponent.GetItemPrefab();
            }

            PlacedItemData placedata = new PlacedItemData(prefab, child.position, child.rotation);
            currentState.placedItems.Add(placedata);
        }
        backupState = currentState.Clone();
        Debug.Log($"백업 : {backupState.placedItems.Count}개 아이템");
    }

    private void RestoreBackupState()
    {
        // 현재 배치된 아이템 제거
        foreach (GameObject obj in currentPlacedItemList)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentPlacedItemList.Clear();

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

                    // PlacedItemController 추가
                    if (restored.GetComponent<PlacedItemController>() == null)
                    {
                        PlacedItemController controller = restored.AddComponent<PlacedItemController>();
                        controller.SetEditManager(this);
                    }


                    currentPlacedItemList.Add(restored);
                }

            }
            Debug.Log($"복원 : {backupState.placedItems.Count}개 아이템");
        }
    }

    private void ApplyCurrentState()
    {
        //현재 배치상태 확정
        currentState.placedItems.Clear();

        foreach (GameObject obj in currentPlacedItemList)
        {
            if (obj != null)
            {
                RoomDecoItem itemComponent = obj.GetComponent<RoomDecoItem>();
                GameObject prefab = null;

                if (itemComponent != null)
                {
                    prefab = itemComponent.GetItemPrefab();
                }

                PlacedItemData placedata = new PlacedItemData(prefab, obj.transform.position, obj.transform.rotation);
                currentState.placedItems.Add(placedata);
            }
        }
        Debug.Log($"상태 적용 : {currentState.placedItems.Count}개 아이템");
    }

    // 초기화 버튼
    private void OnResetButtonClicked()
    {
        Debug.Log("편집 모드 초기화 버튼이 클릭되었습니다.");
        // 모든 배치 아이템 제거
        foreach (GameObject obj in currentPlacedItemList)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentPlacedItemList.Clear();

        DeselectItem();

        Debug.Log("배치가 초기화 완료. 편집모드는 유지");
    }

    // 아이템 배치 개별 삭제 버튼
    private void OnDeleteButtonClicked()
    {
        if (selectedItem != null)
        {
            Debug.Log($"선택된 아이템 삭제: {selectedItem.name}");

            // 리스트에서 제거
            currentPlacedItemList.Remove(selectedItem);

            // GameObject 삭제
            Destroy(selectedItem);

            // 선택 해제
            selectedItem = null;
            selectedItemRenderer = null;
            deleteButton.interactable = false;
        }
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
        currentPlacedItemList.Add(item);

        //PlacedItemController 추가
        if (item.GetComponent<PlacedItemController>() == null)
        {
            PlacedItemController controller = item.AddComponent<PlacedItemController>();
            controller.SetEditManager(this);
        }
    }

    public void SelectPlacedItem(GameObject item)
    {
        // 이전 선택 해제
        DeselectItem();

        selectedItem = item;
        selectedItemRenderer = selectedItem.GetComponent<SpriteRenderer>();

        if (selectedItemRenderer != null)
        {
            // SpriteRenderer.color로 색상 저장
            originalColor = selectedItemRenderer.color;

            // 살짝 강조 색상 적용 (파랗게)
            selectedItemRenderer.color = new Color(
                originalColor.r * 0.8f,
                originalColor.g * 0.8f,
                originalColor.b * 1.2f,
                originalColor.a
            );
        }
        deleteButton.interactable = true;
        Debug.Log($"아이템 선택 : {item.name}");
    }

    public void DeselectItem()
    {
        if (selectedItem != null && selectedItemRenderer != null)
        {
            selectedItemRenderer.color = originalColor;
        }

        selectedItem = null;
        selectedItemRenderer = null;
        deleteButton.interactable = false;
    }

    public Transform GetRoomContainer()
    {
        return roomContainer;
    }
    /*
    public void ResisterPlacedItem(GameObject item)
    {
        currentPlacedObjects.Add(item);
    }
    */
}
