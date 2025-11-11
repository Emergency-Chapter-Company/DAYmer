using UnityEngine;

// 바닥용 그리드
// 마름모 모양
public class RoomDecoGrid_Floor : RoomDecoGrid
{
    [Header("Floor Isometric Settings")]
    [SerializeField] private float isoRatio = 0.5f; // Isometric 비율 (높이/너비)

    // 월드 좌표 => 그리드 좌표
    public override Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        // 원점을 기준으로 변환
        Vector2 localPos = worldPosition - gridOrigin;

        // Isometric 역변환
        // screenX = (gridX - gridY) * tileWidth / 2
        // screenY = (gridX + gridY) * tileHeight / 2
        
        float halfTile = tileSize * 0.5f;
        float isoHeight = tileSize * isoRatio * 0.5f;

        // 역계산
        float girdX = (localPos.x / halfTile + localPos.y / isoHeight) * 0.5f;
        float girdY = (localPos.y / isoHeight - localPos.x / halfTile) * 0.5f;

        

        return new Vector2Int(Mathf.FloorToInt(girdX), Mathf.FloorToInt(girdY));

    }

    // 그리드 좌표 => 월드 좌표
    public override Vector2 GridToWorld(Vector2Int gridPosition)
    {
        float halfTile = tileSize * 0.5f;
        float isoHeight = tileSize * isoRatio * 0.5f;

        //Isometric 변환
        float x = (gridPosition.x - gridPosition.y) * halfTile;
        float y = (gridPosition.x + gridPosition.y) * isoHeight;

        return gridOrigin + new Vector2(x, y);
    }

    // 그리드 내에 있는지 확인
    public override bool IsWithinGrid(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    // 그리드 시각화
    protected override void OnDrawGizmos()
    {
        if (!showGrid)
            return;

        Gizmos.color = gridColor;

        // 그리드 타일 그리기
        // 세로선
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector2 start = GridToWorld(new Vector2Int(x, 0));
            Vector2 end = GridToWorld(new Vector2Int(x, gridHeight));
            Gizmos.DrawLine(start, end);
        }

        // 가로선
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector2 start = GridToWorld(new Vector2Int(0, y));
            Vector2 end = GridToWorld(new Vector2Int(gridWidth, y));
            Gizmos.DrawLine(start, end);
        }
    }
}
