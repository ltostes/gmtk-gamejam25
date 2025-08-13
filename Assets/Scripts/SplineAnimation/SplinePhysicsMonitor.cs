using UnityEngine;
using UnityEngine.Splines;
// using UnityEditor;
using System;

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


    [Header("Force Visualization")]
    public bool showCentripetalVector = true;
    public Color centripetalVectorColor = Color.red;
    public float vectorScale = 0.1f;

    // Public access to forces
    public Vector3 CentripetalForceVector { get; private set; }
    public float CentripetalForceMagnitude { get; private set; }


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
        Vector3 tangent = animator.GetTangentAtPosition();
        Velocity = tangent.normalized * animator.currentSpeed;

        // Calculate curvature
        Curvature = CalculateCurvature(t);
        CentripetalAcceleration = animator.currentSpeed * animator.currentSpeed * Curvature;

        // CALCULATE CENTRIPETAL FORCE VECTOR
        CalculateCentripetalForceVector(t);

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

    void CalculateCentripetalForceVector(float t)
    {
        // Get curvature direction (perpendicular to tangent)
        Vector3 curvatureDirection = GetCurvatureDirection(t);
        
        // Calculate force magnitude: F = m * a = m * (v² / r)
        // Since curvature = 1/r, then F = m * v² * curvature _ For now mass is considered 1
        CentripetalForceMagnitude = 1 * CentripetalAcceleration;
        
        // Create force vector
        CentripetalForceVector = curvatureDirection * CentripetalForceMagnitude;
    }

    Vector3 GetCurvatureDirection(float t)
    {
        // Get current position and tangent
        Vector3 position = transform.position;
        Vector3 tangent_raw = splineContainer.EvaluateTangent(t);
        Vector3 tangent = tangent_raw.normalized;

        // Sample points slightly ahead and behind on the spline
        float delta = Mathf.Clamp(curvatureDelta, 0.001f, 0.1f);
        float t1 = Mathf.Clamp01(t - delta);
        float t2 = Mathf.Clamp01(t + delta);
        
        Vector3 pos1 = splineContainer.EvaluatePosition(t1);
        Vector3 pos2 = splineContainer.EvaluatePosition(t2);

        // Calculate curve normal
        Vector3 chord = (pos2 - pos1).normalized;
        Vector3 curveNormal = Vector3.Cross(tangent, chord).normalized;

        // Determine curvature direction
        Vector3 toCenter = Vector3.Cross(tangent, curveNormal).normalized;

        // Flip direction based on curve orientation
        Vector3 pos_off = splineContainer.EvaluatePosition(t + delta * 0.5f); 
        Vector3 offset = pos_off - position;
        return Vector3.Dot(toCenter, offset) > 0 ? toCenter : -toCenter;
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
            // Handles.color = Color.magenta;
            // Handles.Label(transform.position, $"Curvature: {Curvature:F4}\nRadius: {(1f / Curvature):F1}m");
        }

        if (showAcceleration)
        {
            // GUI.color = CentripetalAcceleration > centripetalAccelThreshold
            //     ? Color.red : Color.yellow;
            // Handles.Label(transform.position + Vector3.up * 0.5f,
            //     $"Centripetal Accel: {CentripetalAcceleration:F1} m/s²\nVelocity: {Velocity.magnitude:F1}m/s");
        }
        
        // Centripetal force vector visualization
        if (showCentripetalVector && CentripetalForceVector != Vector3.zero)
        {
            Gizmos.color = centripetalVectorColor;
            Vector3 start = transform.position;
            Vector3 end = start + CentripetalForceVector * vectorScale;
            
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.1f * vectorScale);
            
            // Add force magnitude label
            // Handles.color = centripetalVectorColor;
            // Handles.Label(end, $"Centripetal: {CentripetalForceMagnitude:F1}N");
        }
    }
}