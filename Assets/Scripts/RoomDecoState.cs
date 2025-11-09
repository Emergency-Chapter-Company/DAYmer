using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlacedItemData
{
    public GameObject itemPrefab;
    public Vector3 position;
    public Quaternion rotation;

    public PlacedItemData(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        itemPrefab = prefab;
        position = pos;
        rotation = rot;
    }
}

public class RoomDecoState
{
    public List<PlacedItemData> placedItems = new List<PlacedItemData>();

    public RoomDecoState clone()
    {
        RoomDecoState clone = new RoomDecoState();
        foreach (var item in placedItems)
        {
            clone.placedItems.Add(new PlacedItemData(
                item.itemPrefab,
                item.position,
                item.rotation
            ));
        }
        return clone;
    }
}