using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

[RequireComponent(typeof(CustomSplineAnimator))]
public class SplinePhysicsMonitor : MonoBehaviour
{
    [Header("Physics Settings")]
    public float centripetalAccelThreshold = 50f;
    public float curvatureDelta = 0.001f;

    [Header("Debug Visualization")]
    public bool showVelocity = true;
    public bool showCurvature = true;
    public bool showAcceleration = true;
    public float debugScale = 1f;

    public Vector3 Velocity { get; private set; }
    public float Curvature { get; private set; }
    public float CentripetalAcceleration { get; private set; }

    private CustomSplineAnimator animator;
    private SplineContainer splineContainer;
    private Vector3 previousPosition;

    void Start()
    {
        animator = GetComponent<CustomSplineAnimator>();
        splineContainer = animator.splineContainer;
        previousPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;
        
        // Calculate velocity using both animator speed and spline direction
        float t = animator.normalizedPosition;
        Vector3 tangent = splineContainer.EvaluateTangent(t);
        Velocity = tangent.normalized * animator.currentSpeed;

        // Calculate curvature
        Curvature = CalculateCurvature(t);
        CentripetalAcceleration = animator.currentSpeed * animator.currentSpeed * Curvature;

        // Check threshold for events
        if (CentripetalAcceleration > centripetalAccelThreshold)
        {
            HandleSharpCurveEvent();
        }

        previousPosition = transform.position;
    }

    float CalculateCurvature(float t)
    {
        // Get first derivative (tangent) at current position
        Vector3 tangent = splineContainer.EvaluateTangent(t);
        
        // Calculate second derivative using finite differences
        float t2 = Mathf.Clamp01(t + curvatureDelta);
        Vector3 nextTangent = splineContainer.EvaluateTangent(t2);
        Vector3 tangentDerivative = (nextTangent - tangent) / curvatureDelta;

        // Curvature = |dT/ds| = |dT/dt| / |dP/dt|
        // Since dP/dt is the tangent vector, its magnitude is the speed
        float speed = tangent.magnitude;
        return tangentDerivative.magnitude / Mathf.Max(speed, 0.0001f);
    }

    void HandleSharpCurveEvent()
    {
        // Implement your custom event logic here
        Debug.Log($"Sharp curve detected! Acceleration: {CentripetalAcceleration:F1}");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || animator == null) return;

        if (showVelocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Velocity * debugScale);
        }

        if (showCurvature)
        {
            Handles.color = Color.magenta;
            Handles.Label(transform.position, $"Curvature: {Curvature:F4}\nRadius: {(1f/Curvature):F1}m");
        }

        if (showAcceleration)
        {
            GUI.color = CentripetalAcceleration > centripetalAccelThreshold 
                ? Color.red : Color.yellow;
            Handles.Label(transform.position + Vector3.up * 0.5f, 
                $"Centripetal Accel: {CentripetalAcceleration:F1} m/s²");
        }
    }
}