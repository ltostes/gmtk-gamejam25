using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

public class SplinePhysicsMonitor : MonoBehaviour
{
    [Header("Physics Settings")]
    public float centripetalAccelThreshold = 50f; // Trigger event when exceeded
    public float curvatureDelta = 0.001f; // Precision for curvature calculation

    [Header("Debug Visualization")]
    public bool showVelocity = true;
    public bool showCurvature = true;
    public bool showAcceleration = true;
    public float debugScale = 1f;

    // Exposed read-only properties for external access
    public Vector3 Velocity { get; private set; }
    public float Curvature { get; private set; }
    public float CentripetalAcceleration { get; private set; }
    public float CurrentSpeed => Velocity.magnitude;

    private SplineAnimate splineAnimate;
    [SerializeField] private SplineContainer splineContainer;
    private Vector3 previousPosition;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        previousPosition = transform.position;

    }
    void FixedUpdate()
    {
        // Calculate linear velocity
        Velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
        previousPosition = transform.position;

        // Calculate curvature if spline is available
        if (splineContainer != null && splineContainer.Spline != null)
        {
            float t = GetNormalizedTime();
            Curvature = CalculateCurvature(t);
            CentripetalAcceleration = CurrentSpeed * CurrentSpeed * Curvature;

            // Check threshold for events
            if (CentripetalAcceleration > centripetalAccelThreshold)
            {
                HandleSharpCurveEvent();
            }
        }
    }
    float GetNormalizedTime()
    {
        // Calculate normalized position along spline (0-1)
        // return Mathf.Clamp01(splineAnimate.ElapsedTime / splineAnimate.Duration);
        return splineAnimate.ElapsedTime / splineAnimate.Duration % 1;
    }

    float CalculateCurvature(float t)
    {
        // Get first derivative (tangent) at current position
        Vector3 tangent = splineContainer.EvaluateTangent(t);

        // Calculate second derivative using finite differences
        float t2 = Mathf.Clamp01(t + curvatureDelta);
        Vector3 nextTangent = splineContainer.EvaluateTangent(t2);
        Vector3 tangentDerivative = (nextTangent - tangent) / curvatureDelta;

        // Curvature = |dT/ds| ≈ |dT/dt| / |dP/dt|
        Vector3 pos = splineContainer.EvaluatePosition(t2);
        float dP_dt = pos.magnitude;
        return tangentDerivative.magnitude / Mathf.Max(dP_dt, 0.0001f);
    }
    void HandleSharpCurveEvent()
    {
        // Implement your custom logic here
        Debug.Log($"Sharp curve detected! Acceleration: {CentripetalAcceleration:F1}");
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (showVelocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Velocity * debugScale);
        }

        if (showCurvature && splineContainer != null)
        {
            Handles.color = Color.magenta;
            Handles.Label(transform.position, $"Curvature: {Curvature:F4}");
        }
        
        if (showCurvature && splineContainer != null)
        {
            Handles.color = Color.white;
            Handles.Label(transform.position + Vector3.up * 0.8f, $"Elapsed time: {GetNormalizedTime():F4}");
        }

        if (showAcceleration)
        {
            GUI.color = CentripetalAcceleration > centripetalAccelThreshold
                ? Color.red : Color.yellow;
            Handles.Label(transform.position + Vector3.up * 0.5f,
                $"Centripetal Accel: {CentripetalAcceleration:F1}");
        }
    }

}
