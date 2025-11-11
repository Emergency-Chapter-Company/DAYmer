using UnityEngine;

public class RoomDecoGrid_LeftWall : RoomDecoGrid
{
    [Header("Wall Settings")]
    [SerializeField] private float skewAngle = 30f; // 기울기 각도

    // 월드 좌표 => 그리드 좌표 (직사각형)
    public override Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2 localPos = worldPosition - gridOrigin;

        // 평행사변형 벽 모양
        float skewFactor = Mathf.Tan(skewAngle * Mathf.Deg2Rad);
        float adjustedX = localPos.x - localPos.y * skewFactor;

        int x = Mathf.FloorToInt(localPos.x / tileSize);
        int y = Mathf.FloorToInt(localPos.y / tileSize);
        return new Vector2Int(x, y);
    }

    // 그리드  좌표 => 월드 좌표 (직사각형)
    public override Vector2 GridToWorld(Vector2Int gridPosition)
    {

        float skewFactor = Mathf.Tan(skewAngle * Mathf.Deg2Rad);

        float x = gridOrigin.x + (gridPosition.x * tileSize) + (gridPosition.y * tileSize * skewFactor) + tileSize * 0.5f;
        float y = gridOrigin.y + (gridPosition.y * tileSize) + tileSize * 0.5f;
        return new Vector2(x, y);
    }

    // 그리드 범위 체크
    public override bool IsWithinGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }


    // 그리드 시각화
    // 씬에서만 보임
    protected override void OnDrawGizmos()
    {

        if (!showGrid)
            return;

        Gizmos.color = gridColor;

        float skewFactor = Mathf.Tan(skewAngle * Mathf.Deg2Rad);

        // 세로선
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector2 start = GridToWorld(new Vector2Int(x, 0));
            Vector2 end = GridToWorld(new Vector2Int(x, gridHeight));
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector2 start = GridToWorld(new Vector2Int(0, y));
            Vector2 end = GridToWorld(new Vector2Int(gridWidth, y));
            Gizmos.DrawLine(start, end);
        }
    }
}

