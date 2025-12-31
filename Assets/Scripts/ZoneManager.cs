using UnityEngine;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour
{
    public float zoneSize = 500f;
    public Transform player;
    public int loadRadius = 1;

    private Dictionary<Vector2Int, List<RoadSegment>> zoneMap = new();
    private HashSet<Vector2Int> activeZones = new();

    void Start()
    {
        BuildZoneMap();
    }

    void Update()
    {
        UpdateActiveZones();
    }

    void BuildZoneMap()
    {
        RoadSegment[] allRoads = FindObjectsOfType<RoadSegment>();
        foreach (var road in allRoads)
        {
            road.AssignZones(zoneSize);
            foreach (var zoneID in road.zones)
            {
                if (!zoneMap.ContainsKey(zoneID))
                    zoneMap[zoneID] = new List<RoadSegment>();
                zoneMap[zoneID].Add(road);
            }
        }
    }

    void UpdateActiveZones()
    {
        if (player == null) return;

        Vector2Int playerZone = ZoneUtility.GetZoneID(player.position, zoneSize);
        HashSet<Vector2Int> newActive = new();

        for (int dx = -loadRadius; dx <= loadRadius; dx++)
            for (int dz = -loadRadius; dz <= loadRadius; dz++)
                newActive.Add(playerZone + new Vector2Int(dx, dz));

        foreach (var zone in newActive)
            SetZoneState(zone, true);

        foreach (var oldZone in activeZones)
            if (!newActive.Contains(oldZone))
                SetZoneState(oldZone, false);

        activeZones = newActive;
    }

    void SetZoneState(Vector2Int zoneID, bool active)
    {
        if (!zoneMap.ContainsKey(zoneID))
            return;

        foreach (var road in zoneMap[zoneID])
            if (road != null)
                road.gameObject.SetActive(active);
    }
}
