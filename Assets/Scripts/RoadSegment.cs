using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class RoadSegment : MonoBehaviour
{
    [Header("Identification")]
    public string roadId;

    [Header("Driving Feel")]
    [Range(0f, 200f)]
    public float speedLimit = 120f;

    public List<float> knotWidths = new();

    [Range(6f, 30f)]
    public float width = 12f;

    [Range(0f, 1f)]
    public float driftAffinity = 0.7f;

    [Header("Rules")]
    public bool allowsRaces = true;
    public bool isHighway = false;

    [Header("Orientation")]
    [Range(-15f, 15f)]
    public float defaultBank = 0f;
    
    [HideInInspector]
    public SplineContainer spline;

    [Header("Streaming")]
    public List<Vector2Int> zones = new List<Vector2Int>();

    public void AssignZones(float zoneSize)
    {
        zones.Clear();

        // Sample the road mesh bounds
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        Bounds b = mf.sharedMesh.bounds;
        Vector3 worldCenter = transform.TransformPoint(b.center);

        Vector2Int centerZone = ZoneUtility.GetZoneID(worldCenter, zoneSize);
        zones.Add(centerZone);

        // Sample corners for long curves
        Vector3[] corners = new Vector3[]
        {
        transform.TransformPoint(b.min),
        transform.TransformPoint(b.max),
        transform.TransformPoint(new Vector3(b.min.x, 0, b.max.z)),
        transform.TransformPoint(new Vector3(b.max.x, 0, b.min.z))
        };

        foreach (var p in corners)
        {
            Vector2Int id = ZoneUtility.GetZoneID(p, zoneSize);
            if (!zones.Contains(id))
                zones.Add(id);
        }
    }

    private void Reset()
    {
        spline = GetComponent<SplineContainer>();
    }

    private void OnValidate()
    {
        if (spline == null)
            spline = GetComponent<SplineContainer>();

        var generator = GetComponent<RoadMeshGenerator>();
        if (generator != null)
            generator.Generate();
    }
}
