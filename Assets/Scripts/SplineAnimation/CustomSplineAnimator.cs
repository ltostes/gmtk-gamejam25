using UnityEngine;
using UnityEngine.Splines;

public class CustomSplineAnimator : MonoBehaviour
{

    [Header("Spline Settings")]
    [SerializeField] public SplineContainer splineContainer;
    public WrapMode wrapMode = WrapMode.Loop;
    public bool alignToSpline = true;

    [Header("Motion Settings")]
    public float maxSpeed = 5f;
    [Range(0f, 1f)] public float normalizedPosition = 0f;
    public float currentSpeed = 0f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Physics Settings")]
    public float gravityInfluence = 0.2f;
    public float friction = 0.05f;

    private float splineLength;
    private float accelerationTimer = 0f;
    private bool isAccelerating = false;
    private Vector3 gravityForce = Vector3.zero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CalculateSplineLength();
        UpdatePositionOnSpline();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handle speed changes smoothly
        if (isAccelerating)
        {
            accelerationTimer += Time.deltaTime;
            currentSpeed = Mathf.Lerp(0, maxSpeed, accelerationCurve.Evaluate(accelerationTimer));
        }

        // Apply physics influences
        ApplyPhysics();

        // Update position along spline
        UpdatePositionOnSpline();
    }


    void CalculateSplineLength(int samples = 100)
    {
        splineLength = 0f;
        Vector3 prevPos = splineContainer.EvaluatePosition(0f);

        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 currentPos = splineContainer.EvaluatePosition(t);
            splineLength += Vector3.Distance(prevPos, currentPos);
            prevPos = currentPos;
        }
    }
    void UpdatePositionOnSpline()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        // Convert speed to normalized progress
        float progressDelta = (currentSpeed * Time.deltaTime) / splineLength;
        normalizedPosition = WrapNormalizedPosition(normalizedPosition + progressDelta);

        // Get and apply position
        Vector3 newPosition = splineContainer.EvaluatePosition(normalizedPosition);
        transform.position = newPosition;

        // Apply rotation if needed
        if (alignToSpline)
        {
            Vector3 tangent = splineContainer.EvaluateTangent(normalizedPosition);
            Vector3 up = splineContainer.EvaluateUpVector(normalizedPosition);
            transform.rotation = Quaternion.LookRotation(tangent, up);
        }
    }
    float WrapNormalizedPosition(float position)
    {
        switch (wrapMode)
        {
            case WrapMode.Loop:
                return Mathf.Repeat(position, 1f);
            case WrapMode.PingPong:
                return Mathf.PingPong(position, 1f);
            default: // Once
                return Mathf.Clamp01(position);
        }
    }
    void ApplyPhysics()
    {
        // Apply friction
        currentSpeed = Mathf.Max(0, currentSpeed - friction * Time.deltaTime);

        // Apply gravity influence
        if (gravityForce != Vector3.zero)
        {
            Vector3 tangent = splineContainer.EvaluateTangent(normalizedPosition);
            Vector3 gravityProjection = Vector3.Project(gravityForce, tangent.normalized);
            currentSpeed += Vector3.Dot(gravityProjection, tangent) * Time.deltaTime;
        }
    }

    // Public methods for controlling motion
    public void SetSpeed(float newSpeed, bool immediate = false)
    {
        maxSpeed = newSpeed;

        if (immediate)
        {
            currentSpeed = newSpeed;
            isAccelerating = false;
        }
        else
        {
            isAccelerating = true;
            accelerationTimer = 0f;
        }
    }

    public void ApplyGravityForce(Vector3 gravity)
    {
        gravityForce = gravity * gravityInfluence;
    }

    public void ApplyImpulse(float impulseForce)
    {
        currentSpeed += impulseForce;
    }

    // For debugging and visualization
    public float GetCurrentVelocity()
    {
        return currentSpeed;
    }

    public float GetNormalizedPosition()
    {
        return normalizedPosition;
    }

}

public enum WrapMode
{
    Once,
    Loop,
    PingPong
}

