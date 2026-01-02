using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Splines;


[System.Serializable]
public class TrackData
{
    public string trackId;
    public string trackName;
    public float zoneSize;
    public List<RoadData> roads;
}

[System.Serializable]
public class RoadData
{
    public string roadId;
    public DrivingData driving;
    public MeshData mesh;
    public SplineData spline;
}

[System.Serializable]
public class DrivingData
{
    public float speedLimit;
    public float width;
    public float driftAffinity;
    public bool allowsRaces;
    public bool isHighway;
    public float defaultBank;
}

[System.Serializable]
public class MeshData
{
    public int segments;
    public float thickness;
    public float curbWidth;
    public float curbHeight;
}

[System.Serializable]
public class SplineData
{
    public bool closed;
    public List<SplineKnotData> knots;
}

[Serializable]
public class SplineKnotData
{
    public Vector3 position;
    public Vector3 tangentIn;
    public Vector3 tangentOut;
    public Quaternion rotation;
    public float width;
}


public class TrackBuilder : MonoBehaviour
{
    public TextAsset trackJson;
    public SplineContainer splinecontainer;

    public void Build()
    {
        TrackData track = JsonUtility.FromJson<TrackData>(trackJson.text);

        Debug.Log("We are here!");
        foreach (var road in track.roads)
        {
            GameObject roadGO = new GameObject(road.roadId);
            roadGO.transform.parent = transform;

            var splineContainer = roadGO.AddComponent<SplineContainer>();
            var roadSegment = roadGO.AddComponent<RoadSegment>();
            var meshGen = roadGO.AddComponent<RoadMeshGenerator>();
            roadGO.AddComponent<MeshCollider>();

            Debug.Log("RoadSegment");
            // --- RoadSegment ---
            roadSegment.roadId = road.roadId;
            roadSegment.speedLimit = road.driving.speedLimit;
            roadSegment.width = road.driving.width;
            roadSegment.driftAffinity = road.driving.driftAffinity;
            roadSegment.allowsRaces = road.driving.allowsRaces;
            roadSegment.isHighway = road.driving.isHighway;
            roadSegment.defaultBank = road.driving.defaultBank;

            Debug.Log("Mesh");
            // --- Mesh ---
            meshGen.segments = road.mesh.segments;
            meshGen.thickness = road.mesh.thickness;
            meshGen.curbWidth = road.mesh.curbWidth;
            meshGen.curbHeight = road.mesh.curbHeight;

            Debug.Log("Spline");

            // --- Spline ---
            Spline spline = new Spline();
            spline.Closed = road.spline.closed;

            foreach (var k in road.spline.knots)
            {
                BezierKnot knot = new BezierKnot(
                    k.position,
                    k.tangentIn,
                    k.tangentOut,
                    k.rotation
                );
                spline.Add(knot);
            }

            roadSegment.knotWidths.Clear();

            foreach (var k in road.spline.knots)
            {
                float w = k.width > 0 ? k.width : road.driving.width;
                roadSegment.knotWidths.Add(w);
            }


            splineContainer.Splines = new List<Spline> { spline };

            // Build mesh
            meshGen.Generate();

            // Assign streaming zones
            roadSegment.AssignZones(track.zoneSize);
        }
    }

    public void SaveSplineToJson()
    {
        string fileName = "jsonTest";
        Debug.Log("1");
        Spline spline = splinecontainer.Spline;
        if (spline == null)
        {
            Debug.LogError("Spline is null.");
            return;
        }

        SplineData splineData = new SplineData();
        if (splineData == null)
        {
            Debug.LogError("SplineData is null.");
            return;
        }

        splineData.knots = new();

        foreach (var knot in spline.Knots)
        {
            splineData.knots.Add(new SplineKnotData
            {
                position = knot.Position,
                rotation = knot.Rotation,
                tangentIn = knot.TangentIn,
                tangentOut = knot.TangentOut,
                width = 10
            });
        }
        Debug.Log("3");

        string json = JsonUtility.ToJson(splineData, true);
        string path = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(path, json);
        Debug.Log($"Spline saved to {path}");
    }

}
