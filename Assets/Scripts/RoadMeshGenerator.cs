using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
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
        if (mf.sharedMesh == null)
        {
            mesh = new Mesh();
            mesh.name = "RoadMesh";
            mf.sharedMesh = mesh;
        }
        else
        {
            mesh = mf.sharedMesh;
        }

        Generate();
    }

    void OnValidate()
    {
        Generate();
    }

    public void Generate()
    {
        if (road == null || container == null || container.Splines.Count == 0)
            return;

        mesh.Clear();

        var spline = container.Splines[0];
        float width = road.width * 0.5f;

        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector3> normals = new();

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;

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
