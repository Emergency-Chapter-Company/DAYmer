using UnityEngine;

public class PlacedItemController : MonoBehaviour
{
    private RoomDecoEdit editManager;
    private bool isDragging = false;
    private Vector3 offset;
    //private SpriteRenderer spriteRenderer;

    //private void Awake()
    //{
    //    spriteRenderer = GetComponent<SpriteRenderer>();
    //}

    public void SetEditManager(RoomDecoEdit manager)
    {
        editManager = manager;
    }

    private void OnMouseDown()
    {
        Debug.Log($"OnMouseDown 호출됨 : {gameObject.name}");

        if (editManager == null || !editManager.IsEditMode())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            editManager.SelectPlacedItem(gameObject);

            // 드래그 준비
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            offset = transform.position - mousePos;
            isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        if (editManager == null || !editManager.IsEditMode())
            return;

        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos + offset;

        }
    }
    private void OnMouseUp()
    {
        isDragging = false;
    }
}
