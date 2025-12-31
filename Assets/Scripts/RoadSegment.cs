using UnityEngine;
using UnityEngine.Splines;

[DisallowMultipleComponent]
public class RoadSegment : MonoBehaviour
{
    [Header("Identification")]
    public string roadId;

    [Header("Driving Feel")]
    [Range(0f, 200f)]
    public float speedLimit = 120f;

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
