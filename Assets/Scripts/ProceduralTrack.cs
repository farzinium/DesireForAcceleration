using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

public class ProceduralTrack : MonoBehaviour
{
    public SplineContainer splineContainer;
    public int numKnots = 20; // For a ~2-5km loop
    public float trackLength = 3000f; // Desired total length
    public Vector2 bounds = new(500, 300); // Track area

    void Start()
    {
        splineContainer = gameObject.AddComponent<SplineContainer>();
        Spline spline = new() { IsClosed = true };

        for (int i = 0; i < numKnots; i++)
        {
            float t = (float)i / numKnots;
            Vector3 pos = new(
                Random.Range(-bounds.x, bounds.x),
                0,
                Mathf.PerlinNoise(t * 3, 0) * 50  // Gentle hills
            );
            spline.Knots.Append(new BezierKnot(pos, Vector3.up * Random.Range(20, 50)));  // Random tangents for curves
        }

        // Smooth & normalize length (iterate to adjust knots)
        splineContainer.Splines[0] = spline;
    }
}