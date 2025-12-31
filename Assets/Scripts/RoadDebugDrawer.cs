using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class RoadDebugDrawer : MonoBehaviour
{
    public Color roadColor = new Color(1f, 1f, 0f, 0.8f);
    public int samplesPerSpline = 40;

    RoadSegment road;
    SplineContainer container;

    void OnEnable()
    {
        road = GetComponent<RoadSegment>();
        container = GetComponent<SplineContainer>();
    }

    void OnDrawGizmos()
    {
        if (road == null || container == null || container.Splines.Count == 0)
            return;

        Gizmos.color = roadColor;

        var spline = container.Splines[0];
        float width = road.width;

        Vector3 prevLeft = Vector3.zero;
        Vector3 prevRight = Vector3.zero;
        bool hasPrev = false;

        for (int i = 0; i <= samplesPerSpline; i++)
        {
            float t = i / (float)samplesPerSpline;

            Vector3 center = spline.EvaluatePosition(t);
            Vector3 tangent = ((Vector3)spline.EvaluateTangent(t)).normalized; 
            Vector3 up = Vector3.up;
            float bank = road.defaultBank;
            Quaternion bankRot = Quaternion.AngleAxis(bank, tangent);
            up = bankRot * up;

            Vector3 right = Vector3.Cross(up, tangent).normalized;

            Vector3 leftEdge = center - right * (width * 0.5f);
            Vector3 rightEdge = center + right * (width * 0.5f);

            Gizmos.DrawSphere(leftEdge, 0.2f);
            Gizmos.DrawSphere(rightEdge, 0.2f);

            if (hasPrev)
            {
                Gizmos.DrawLine(prevLeft, leftEdge);
                Gizmos.DrawLine(prevRight, rightEdge);
            }

            prevLeft = leftEdge;
            prevRight = rightEdge;
            hasPrev = true;
        }
    }
}
