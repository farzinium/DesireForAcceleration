using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class RoadMeshGenerator : MonoBehaviour
{
    [Header("Roads")]
    public int segments = 50;
    public float thickness = 0.2f;

    [Header("Curbs")]
    public float curbWidth = 0.6f;
    public float curbHeight = 0.15f;

    RoadSegment road;
    SplineContainer container;
    Mesh mesh;
    MeshCollider meshCollider;

    void OnEnable()
    {
        road = GetComponent<RoadSegment>();
        container = GetComponent<SplineContainer>();
        meshCollider = GetComponent<MeshCollider>();

        var mf = GetComponent<MeshFilter>();
        mesh = new();
        mesh.name = "RoadMesh_Instance";
        mf.mesh = mesh; // IMPORTANT: use mesh, not sharedMesh

        Generate();
    }

    void OnValidate()
    {
        Generate();
    }

    float EvaluateWidth(float t)
    {
        if (road.knotWidths == null || road.knotWidths.Count == 0)
            return road.width;

        float scaled = t * (road.knotWidths.Count - 1);
        int i0 = Mathf.FloorToInt(scaled);
        int i1 = Mathf.Clamp(i0 + 1, 0, road.knotWidths.Count - 1);
        float lerp = scaled - i0;

        return Mathf.Lerp(road.knotWidths[i0], road.knotWidths[i1], lerp);
    }


    public void Generate()
    {
        if (road == null || container == null || container.Splines.Count == 0)
            return;

        mesh.Clear();

        var spline = container.Splines[0]; 

        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector3> normals = new();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float width = EvaluateWidth(t) * 0.5f;

            Vector3 center = spline.EvaluatePosition(t);
            Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized;

            Vector3 up = Vector3.up;
            Quaternion bankRot = Quaternion.AngleAxis(road.defaultBank, tangent);
            up = bankRot * up;

            Vector3 right = Vector3.Cross(up, tangent).normalized;

            Vector3 roadLeft = center - right * width;
            Vector3 roadRight = center + right * width;

            // Curb positions
            Vector3 curbLeftOuter = roadLeft - right * curbWidth + up * curbHeight;
            Vector3 curbRightOuter = roadRight + right * curbWidth + up * curbHeight;

            // Road surface
            verts.Add(roadLeft);
            verts.Add(roadRight);

            // Curb tops
            verts.Add(curbLeftOuter);
            verts.Add(curbRightOuter);

            normals.Add(up);
            normals.Add(up);
            normals.Add(up);
            normals.Add(up);
        }

        for (int i = 0; i < segments; i++)
        {
            int idx = i * 4;

            // Road surface
            tris.Add(idx);
            tris.Add(idx + 4);
            tris.Add(idx + 1);

            tris.Add(idx + 1);
            tris.Add(idx + 4);
            tris.Add(idx + 5);

            // Left curb
            tris.Add(idx);
            tris.Add(idx + 2);
            tris.Add(idx + 4);

            tris.Add(idx + 4);
            tris.Add(idx + 2);
            tris.Add(idx + 6);

            // Right curb
            tris.Add(idx + 1);
            tris.Add(idx + 5);
            tris.Add(idx + 3);

            tris.Add(idx + 3);
            tris.Add(idx + 5);
            tris.Add(idx + 7);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetNormals(normals);
        mesh.RecalculateBounds(); 
        
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}
