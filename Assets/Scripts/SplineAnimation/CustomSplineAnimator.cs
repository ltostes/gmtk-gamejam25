using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem;

public class CustomSplineAnimator : MonoBehaviour
{

    [Header("Spline Settings")]
    [SerializeField] public SplineContainer splineContainer;
    public WrapMode wrapMode = WrapMode.Loop;
    public bool alignToSpline = true;

    [Header("Motion Settings")]
    public float initialPosition = 0.035f;
    public float initialSpeed = 0f;
    public float maxSpeed = 5f;
    [Range(0f, 1f)] public float normalizedPosition = 0f;
    public float currentSpeed = 0.01f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Physics Settings")]
    public float gravityInfluence = 0.2f;
    public float friction = 0.05f;

    private float splineLength;
    private float accelerationTimer = 0f;
    private bool isAccelerating = false;
    private Vector3 gravityForce = Vector3.zero;


    [Header("Input Controls")]
    public float accelerationForce = 5f;
    public float brakingForce = 10f;
    public float maxAcceleration = 2f; // Multiplier for max speed during acceleration

    // Physics forces storage
    public float _currentAcceleration;
    public float _currentBraking;

    [Header("Input System")]
    public InputActionAsset inputActions;
    private InputAction accelerateAction;
    private InputAction brakeAction;

    [Header("Manager References")]
    public TrackObjectManager trackObjectManager;
    public StateController stateController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CalculateSplineLength();
        UpdatePositionOnSpline(true); // isStart = true
        SetupInputActions();
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset is not assigned!");
            return;
        }

        // Find the actions
        var vehicleMap = inputActions.FindActionMap("Vehicle");
        if (vehicleMap != null)
        {
            accelerateAction = vehicleMap.FindAction("Accelerate");
            brakeAction = vehicleMap.FindAction("Brake");

            if (accelerateAction != null) accelerateAction.Enable();
            if (brakeAction != null) brakeAction.Enable();
        }
    }

    void OnDestroy()
    {
        if (accelerateAction != null) accelerateAction.Disable();
        if (brakeAction != null) brakeAction.Disable();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Reset forces each frame
        _currentAcceleration = 0f;
        _currentBraking = 0f;

        // Read input using Input System
        bool isAccelerating = accelerateAction != null && accelerateAction.ReadValue<float>() > 0.1f && stateController.isActive;
        bool isBraking = brakeAction != null && brakeAction.ReadValue<float>() > 0.1f && stateController.isActive;

        if (isAccelerating)
        {
            _currentAcceleration = accelerationForce;
        }

        if (isBraking)
        {
            _currentBraking = brakingForce;
        }
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

        // Track segments such as lifts, brakes etc
        ApplyTrackObjects();

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
    void UpdatePositionOnSpline(bool isStart = false)
    {
        if (splineContainer == null
            || splineContainer.Spline == null
            || (currentSpeed == 0 && !isStart)
        ) return;

        // Convert speed to normalized progress
        float progressDelta = (currentSpeed * Time.deltaTime) / splineLength;
        normalizedPosition = Mathf.Repeat(normalizedPosition + progressDelta, 1f); // Repeat is for it to loop after finish

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

    void ApplyPhysics()
    {
        // Calculate acceleration forces
        Vector3 tangent = GetTangentAtPosition();

        // Gravity projection
        Vector3 gravityProjection = Vector3.Project(Physics.gravity, tangent);
        float gravityAcceleration = Vector3.Dot(gravityProjection, tangent) * gravityInfluence;

        // Curvature forces (from physics monitor)
        SplinePhysicsMonitor monitor = GetComponent<SplinePhysicsMonitor>();
        float centripetalEffect = monitor != null ? monitor.CentripetalAcceleration * 0.1f : 0f;

        // Apply input forces
        float inputForce = _currentAcceleration - _currentBraking;

        // Combined acceleration
        float totalAcceleration = gravityAcceleration - friction + inputForce; // - centripetalEffect;

        currentSpeed = Mathf.Max(0, currentSpeed + totalAcceleration * Time.deltaTime);

        // Enforce speed limits
        float effectiveMaxSpeed = _currentAcceleration > 0 ? maxSpeed * maxAcceleration : maxSpeed;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, effectiveMaxSpeed);
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
    private void ApplyTrackObjects()
    {
        if (trackObjectManager == null) return;

        TrackSegment activeSegment = trackObjectManager.GetActiveSegment(normalizedPosition);
        if (activeSegment == null) return;

        switch (activeSegment.type)
        {
            case TrackObjectType.Lift:
                // Maintain minimum speed for lifts
                currentSpeed = Mathf.Max(currentSpeed, activeSegment.liftSpeed);
                break;

            case TrackObjectType.Brake:
                // Apply constant braking force
                currentSpeed -= activeSegment.brakeStrength * Time.deltaTime;
                currentSpeed = Mathf.Max(activeSegment.brakeMinSpeed, currentSpeed);
                break;

            case TrackObjectType.Booster:
                // Apply instant speed boost
                currentSpeed += activeSegment.boosterForce * Time.deltaTime;
                break;

            case TrackObjectType.Checkpoint:
                // You could implement checkpoint logic here
                break;
        }
    }

    public Vector3 GetTangentAtPosition()
    {
        if (splineContainer == null) return Vector3.forward;
        Vector3 ret_tangent = splineContainer.EvaluateTangent(normalizedPosition);
        return ret_tangent.normalized;
    }

    [ContextMenu("Reset Position")]
    public void resetPosition()
    {
        normalizedPosition = initialPosition;
        currentSpeed = initialSpeed;
        UpdatePositionOnSpline(true);
    }

}
