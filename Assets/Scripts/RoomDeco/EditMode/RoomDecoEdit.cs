using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomDecoEdit : MonoBehaviour
{
    /* ====== 객체 변수 ====== */
    private GameManager gameManager;

    /* ====== 편집 모드 상태 변수 ====== */
    private RoomDecoState currentState = new RoomDecoState();
    private RoomDecoState backupState = null;
    private List<GameObject> currentPlacedItemList = new List<GameObject>();
    private bool isEditMode = false;

    /* ====== 선택된 아이템 변수 ====== */
    private GameObject selectedItem = null;
    private SpriteRenderer selectedItemRenderer = null;
    private Color originalColor;

    /* ====== UI 참조 변수 ====== */
    [Header("UI References")]
    [SerializeField] private Button editModeButton;
    [SerializeField] private Button storeButton;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject editModeUI;      // 취소/확인 버튼 포함하기

    [Header("Edit Mode UI")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button deleteButton;

    [Header("Room Management")]
    [SerializeField] private Transform roomContainer; //아이템들이 배치될 부모 오브젝트

    private void Awake()
    {
        gameManager = GameManager.instance;
    }

    private void Start()
    {
        /* 버튼 리스너 등록 */
        editModeButton.onClick.AddListener(EnterEditMode);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        resetButton.onClick.AddListener(OnResetButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);

        /* 초기 상태 설정 : 편집 모드 비활성화 */
        inventoryPanel.SetActive(false);
        editModeUI.SetActive(false);
        deleteButton.interactable = false;   // 삭제버튼 초기 비활성화

        /* GameManager에 자신 등록 */
        if (gameManager == null)
            gameManager = GameManager.instance; // 두 번째 안전 체크

        if (gameManager != null)
            gameManager.RegisterRoomDecoEdit(this);
        else
            Debug.LogError("[RoomDecoEdit] GameManager 인스턴스 없음");
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
        storeButton.gameObject.SetActive(false);
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
        storeButton.gameObject.SetActive(true);
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
        //Debug.Log($"백업 : {backupState.placedItems.Count}개 아이템");
    }

    private void RestoreBackupState()
    {
        // 현재 아이템 개수와 백업 개수 비교
        int backupCount = backupState.placedItems.Count;
        int currentCount = currentPlacedItemList.Count;

        // 현재 아이템이 더 많으면 초과분 삭제
        for (int i = backupCount; i < currentCount; i++)
        {
            if (currentPlacedItemList[i] != null)
                Destroy(currentPlacedItemList[i]);
        }

        // 아이템 리스트 정리
        if (currentCount > backupCount)
            currentPlacedItemList.RemoveRange(backupCount, currentCount - backupCount);

        // 기존 아이템 유지하면서 상태만 복원
        for (int i = 0; i < backupCount; i++)
        {
            PlacedItemData data = backupState.placedItems[i];

            if (i < currentPlacedItemList.Count && currentPlacedItemList[i] != null)
            {
                GameObject existing = currentPlacedItemList[i];
                existing.transform.position = data.position;
                existing.transform.rotation = data.rotation;

                // 컨트롤러 재연결
                var controller = existing.GetComponent<PlacedItemController>();
                if (controller == null)
                    controller = existing.AddComponent<PlacedItemController>();
                controller.SetEditManager(this);
            }
            else
            {
                // 없는 경우 새로 추가
                if (data.itemPrefab != null)
                {
                    GameObject restored = Instantiate(data.itemPrefab, roomContainer);
                    restored.transform.position = data.position;
                    restored.transform.rotation = data.rotation;

                    var controller = restored.GetComponent<PlacedItemController>();
                    if (controller == null)
                        controller = restored.AddComponent<PlacedItemController>();
                    controller.SetEditManager(this);

                    currentPlacedItemList.Add(restored);
                }
            }
        }

        //Debug.Log($"복원 완료: {backupCount}개 아이템 복원");
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
        //Debug.Log($"상태 적용 : {currentState.placedItems.Count}개 아이템");
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

        //Debug.Log("배치가 초기화 완료. 편집모드는 유지");
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

        /* 이전 상태로 상대 복원 */
        RestoreBackupState();
        ExitEditMode();
    }


    private void OnConfirmButtonClicked()
    {
        Debug.Log("편집 모드 확인 버튼이 클릭되었습니다.");

        /* 현재 상태 저장 */
        ApplyCurrentState();
        GameManager.instance.SaveGame();
        ExitEditMode();
    }

    // 외부에서 아이템 추가 시 호출
    public void AddPlacedItem(GameObject item)
    {
        currentPlacedItemList.Add(item);

        // 콜라이더 없으면 자동 추가
        if (item.GetComponent<Collider2D>() == null)
        {
            var col = item.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }

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

    //public void ResisterPlacedItem(GameObject item)
    //{
    //    currentPlacedObjects.Add(item);
    //}

    public List<PlacedItemSaveData> GetRoomStateForSave()
    {
        List<PlacedItemSaveData> result = new();

        foreach (var obj in currentPlacedItemList)
        {
            var item = obj.GetComponent<RoomDecoItem>();
            if (item == null) continue;

            result.Add(new PlacedItemSaveData()
            {
                itemID = item.GetItemID(),
                position = obj.transform.position,
                rotation = obj.transform.rotation
            });
        }

        return result;
    }

    public void LoadRoomState(List<PlacedItemSaveData> loaded)
    {
        if (loaded == null) return;

        // 기존 배치 초기화
        foreach (var obj in currentPlacedItemList)
            Destroy(obj);

        currentPlacedItemList.Clear();

        var storeManager = GetComponent<StoreManager>();

        foreach (var saved in loaded)
        {
            RoomDecoItem prefab = storeManager.GetItemByID(saved.itemID);
            if (prefab == null)
            {
                Debug.LogWarning($"로드 실패: itemID {saved.itemID} 찾을 수 없음");
                continue;
            }

            GameObject obj = Instantiate(prefab.gameObject, saved.position, saved.rotation, roomContainer);
            AddPlacedItem(obj);
        }

        ApplyCurrentState();
    }
}
