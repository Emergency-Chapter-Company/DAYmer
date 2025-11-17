using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;   

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /* ====== 아이템 속성 ====== */
    private RoomDecoItem DecoItem;
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
        editModeManager = FindAnyObjectByType<RoomDecoEdit>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 아이콘 생성
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        // 드래그 미리보기 아이콘 생성
        draggedObject = new GameObject("DraggingIcon");
        draggedObject.transform.SetParent(canvas.transform, false);

        Image image = draggedObject.AddComponent<Image>();

        // RoomDecoItem 프리팹의 스프라이트 직접 사용
        if (DecoItem != null)
        {
            SpriteRenderer sr = DecoItem.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                image.sprite = sr.sprite; // 프리팹의 실제 스프라이트
                image.color = sr.color; // 프리팹의 실제 색상
            }
            else
            {
                image.sprite = GetComponent<Image>().sprite; // fallback
                image.color = Color.white;
            }
        }
        else
        {
            image.sprite = GetComponent<Image>().sprite; // fallback
            image.color = Color.white;
        }

        image.preserveAspect = true;
        image.raycastTarget = false;

        RectTransform dragRect = draggedObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = new Vector2(70, 70); // 살짝 작게
        dragRect.pivot = new Vector2(0.5f, 0.5f);

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

        // 마우스 위치에 아이템 배치
        if (editModeManager != null && DecoItem.GetItemPrefab() != null)
        {
            Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spawnPosition.z = 0;

            GameObject placedItem = Instantiate(DecoItem.GetItemPrefab());
            placedItem.transform.position = spawnPosition;
            placedItem.transform.SetParent(editModeManager.GetRoomContainer());

            RoomDecoItem itemComponent = placedItem.GetComponent<RoomDecoItem>();
            if (itemComponent ==null)
            {
                itemComponent = placedItem.AddComponent<RoomDecoItem>();
            }
            //itemComponent.SetItemData(DecoItem);

            // 아이템선택을 하려면 아이템 프리펩에 Box콜라이더2D가 설정되어야 하는데
            // 이걸 자동으로 해주는 코드
            // 일단 이거 없이 해보니까 선택이 안되가지고 넣음
            // 코드가 아니더라도 프리펩에서 설정하면 되긴하는데 자동 코드가 편할 것 같음
            if (placedItem.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D collider =placedItem.AddComponent<BoxCollider2D>();

                //스프라이트 크기에 맞춰 자동 조정됨
                Debug.Log($"Collider2D 자동 추가 : {placedItem.name}");
            }

            ///* 스프라이트 렌더러의 머티리얼과 색상 설정 */
            //SpriteRenderer sr = placedItem.GetComponentInChildren<SpriteRenderer>();
            //if (sr != null)
            //{
            //    sr.color = DecoItem.GetItemColor(); // 혹은 Color.white
            //}

            // 편집 모드 매니저에 배치된 아이템 등록
            editModeManager.AddPlacedItem(placedItem);
        }

        // 실제 배치 구현 여기추가예정
        Debug.Log($"드래그 종료: {DecoItem.GetItemName()}");
    }

    public void SetItemData(RoomDecoItem item)
    {
        DecoItem = item;
    }
}
