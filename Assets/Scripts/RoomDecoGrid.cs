using System.Collections.Generic;
using UnityEngine;

// 2D 그리드 시스템

public class RoomDecoGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Vector2 gridOrigin = new Vector2(0, 0);

    [Header("Visualization")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Occupancy")]
    private Dictionary<Vector2Int, ItemType> occupiedTiles = new Dictionary<Vector2Int, ItemType>();

    // 월드 => 그리드 좌표 계산
    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2 localPos = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPos.x / tileSize);
        int y = Mathf.FloorToInt(localPos.y / tileSize);
        return new Vector2Int(x, y);
    }

    // 그리드 => 월드 좌표 계산
    public Vector2 GridToWorld(Vector2Int gridPosition)
    {
        float x = gridOrigin.x + (gridPosition.x * tileSize) + tileSize * 0.5f;
        float y = gridOrigin.y + (gridPosition.y * tileSize) + tileSize * 0.5f;
        return new Vector2(x, y);
    }

    //그리드 범위 내 확인
    public bool IsWithinGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    private void OnDrawGizmos()
    {
        if (!showGrid) return;

        Gizmos.color = gridColor;

        // 세로선
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector2 start = gridOrigin + new Vector2(x * tileSize, 0);
            Vector2 end = gridOrigin + new Vector2(x * tileSize, gridHeight * tileSize);
            Gizmos.DrawLine(start, end);
        }

        //가로선
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector2 start = gridOrigin + new Vector2(0, y * tileSize);
            Vector2 end = gridOrigin + new Vector2(gridWidth * tileSize, y * tileSize);
            Gizmos.DrawLine(start, end);
        }
    }

    // 그리드에 아이템 배치 여부확인
    public bool IsTileOccupied(Vector2Int gridPos, ItemType itemType)
    {
        if(!occupiedTiles.ContainsKey(gridPos))
            return false;

        //같은 레이어에서만 중복 체크
        return occupiedTiles[gridPos] == itemType;
    }

    // 그리드에 아이템 배치 등록
    public void OccupyTile(Vector2Int gridPos, ItemType itemType)
    {
        occupiedTiles[gridPos] = itemType; 
    }

    // 그리드에서 아이템 배치 해제
    public void FreeTile(Vector2Int gridPos)
    {
        occupiedTiles.Remove(gridPos);
    }


    public int GetGridWidth() => gridWidth;
        public int GetGridHeight() => gridHeight;
        public float GetTileSize() => tileSize;
}
