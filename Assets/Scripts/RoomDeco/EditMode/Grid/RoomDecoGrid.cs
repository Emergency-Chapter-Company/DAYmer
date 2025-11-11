using System.Collections.Generic;
using UnityEngine;

// 2D 그리드 시스템

//그리드 시스템 추상 클래스
//모든 그리드 시스템은 이 클래스를 상속받아야 함
public abstract class RoomDecoGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] protected int gridWidth = 10;
    [SerializeField] protected int gridHeight = 10;
    [SerializeField] protected float tileSize = 1f;
    [SerializeField] protected Vector2 gridOrigin = new Vector2(0, 0);

    [Header("Visualization")]
    [SerializeField] protected bool showGrid = true;
    [SerializeField] protected Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Occupancy")]
    protected Dictionary<Vector2Int, ItemType> occupiedTiles = new Dictionary<Vector2Int, ItemType>();

    // 개별 - 추상. 각각의 그리드에서 구현해야 되는 부분
    public abstract bool IsWithinGrid(Vector2Int gridPosition);
    public abstract Vector2Int WorldToGrid(Vector2 worldPosition);
    public abstract Vector2 GridToWorld(Vector2Int gridPosition);

    // 공통 - 모든 그리드에서 구현 필요
    public bool IsTileOccupied(Vector2Int gridPos, ItemType itemType)
    {
        if (!occupiedTiles.ContainsKey(gridPos))
            return false;

        // 같은 레이어에서만 중복 체크
        return occupiedTiles[gridPos] == itemType;
    }

    public void OccupyTile(Vector2Int gridPos, ItemType itemType)
    {
        occupiedTiles[gridPos] = itemType;
    }

    public void FreeTile(Vector2Int gridPos)
    {
        occupiedTiles.Remove(gridPos);
    }

    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;
    public float GetTileSize() => tileSize;

    // 그리드 그리기는 각 그리드에서 구현
    protected abstract void OnDrawGizmos();
}
