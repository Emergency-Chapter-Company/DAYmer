using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;   

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public ItemData2D itemData;

    
    private GameObject draggedObject;
    private RoomDecoEdit editModeManager;
    private GameObject draggingIcon;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        // 캔버스 그룹 추가 (드래그 중에 아이템이 다른 UI 요소와 상호작용하지 않도록)
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 이 부분은 지워도 되는데 일단 드래그로 컴포넌트 지정하는 거 혹시 오류 생길까봐 남겨는 둠
        // 근데 지워도 될 것 같긴 해
        // 지울까? 말까? 어카지
        editModeManager = FindObjectOfType<RoomDecoEdit>();         
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 시작 -  드래그 아이콘 생성
        draggedObject = new GameObject("DraggingIcon");
        draggedObject.transform.SetParent(canvas.transform);

        Image image = draggedObject.AddComponent<Image>();
        image.sprite = GetComponent<Image>().sprite;
        image.raycastTarget = false;        // 드래그 아이콘이 다른 UI 요소와 상호작용하지 않도록 설정
                                            
        RectTransform dragRect = draggedObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = new Vector2(100, 100); // 원하는 크기로 설정

        //원본 슬롯 반투명
        
        canvasGroup.alpha = 0.6f;
        
        Debug.Log("드래그 기능 활성");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그 중 마우스 위치 따라가기
        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;
        }
    }    

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 - 드래그 아이콘 제거
        if (draggedObject != null)
        {
            Destroy(draggedObject);
        }

        //원본 슬롯 불투명하게 복원
        canvasGroup.alpha = 1.0f;

        //디버깅용잠깐쓰는거
        Debug.Log($"editModeManager null? {editModeManager == null}");
        Debug.Log($"itemData null? {itemData == null}");
        if (itemData != null)
        {
            Debug.Log($"itemData.ItemPrefab null? {itemData.ItemPrefab == null}");
        }


        // 마우스 위치에 아이템 배치
        if (editModeManager != null && itemData.ItemPrefab != null)
        {

            Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spawnPosition.z = 0;

            GameObject placedItem = Instantiate(itemData.ItemPrefab);
            placedItem.transform.position = spawnPosition;
            placedItem.transform.SetParent(editModeManager.GetRoomContainer());
            
            // 편집 모드 매니저에 배치된 아이템 등록
            editModeManager.AddPlacedItem(placedItem);
        }

        // 실제 배치 구현 여기추가예정
        Debug.Log($"드래그 종료: {itemData.ItemName}");
    }
}
