using UnityEngine;
using UnityEngine.Splines;
using System.Collections;
using UnityEngine.Rendering;

public class CinematicSplineCamera : MonoBehaviour
{
    [Header("Spline")]
    public SplineContainer splineContainer;
    public float moveSpeed = 2f;

    [Header("Look At")]
    public Transform lookTarget;

    [Header("Timing")]
    public float pauseAtEnd = 1f;
    public float K = 1f;

    int currentSplineIndex = 0;
    float t = 0f;
    bool isMoving = true;

    void Start()
    {
        if (splineContainer == null || splineContainer.Splines.Count == 0)
        {
            Debug.LogError("SplineContainer is missing or has no splines!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!isMoving)
        {
            
            return;
        }

        Spline spline = splineContainer.Splines[currentSplineIndex];
        float splineLength = spline.GetLength();

        t += (K * moveSpeed / splineLength) * Time.deltaTime;
        t = Mathf.Clamp01(t);
        Vector3 position = splineContainer.EvaluatePosition(currentSplineIndex, t);
        transform.position = position;

        if (lookTarget != null)
        {
            Vector3 dir = lookTarget.position - transform.position;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 5f
                );
        }

        if (t >= 1f)
        {
            StartCoroutine(PauseAndNextSpline());
        }
    }

    IEnumerator PauseAndNextSpline()
    {
        isMoving = false;
        yield return new WaitForSeconds(pauseAtEnd);

        t = 0f;
        currentSplineIndex = (currentSplineIndex + 1) % splineContainer.Splines.Count;
        isMoving = true;
    }
}
