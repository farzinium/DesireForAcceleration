using UnityEngine;

public static class ZoneUtility
{
    public static Vector2Int GetZoneID(Vector3 position, float zoneSize)
    {
        int x = Mathf.FloorToInt(position.x / zoneSize);
        int z = Mathf.FloorToInt(position.z / zoneSize);
        return new Vector2Int(x, z);
    }
}
