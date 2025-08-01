using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.InputSystem; // For the new Input System (optional)

public class AdvancedSplineControl : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float accelerationRate = 1.5f;
    [SerializeField] private float decelerationRate = 2f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float speedSmoothing = 5f;

    [Header("Input Settings")]
    [SerializeField] private KeyCode accelerateKey = KeyCode.Space;
    [SerializeField] private KeyCode decelerateKey = KeyCode.B;

    private SplineAnimate splineAnimate;
    private float targetSpeed;
    private float currentSpeed;

    void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        targetSpeed = baseSpeed;
        currentSpeed = baseSpeed;
        splineAnimate.MaxSpeed = baseSpeed;
    }

    void Update()
    {
        HandleInput();
        SmoothSpeedChange();
        UpdateAnimation();
    }

    private void HandleInput()
    {
        // Acceleration
        if (Input.GetKey(accelerateKey))
        {
            targetSpeed = Mathf.Min(targetSpeed + accelerationRate * Time.deltaTime, maxSpeed);
        }
        // Deceleration
        else if (Input.GetKey(decelerateKey))
        {
            targetSpeed = Mathf.Max(targetSpeed - decelerationRate * Time.deltaTime, minSpeed);
        }
        // Optional: Return to base speed when no keys are pressed
        else
        {
            targetSpeed = Mathf.MoveTowards(targetSpeed, baseSpeed, decelerationRate * 0.5f * Time.deltaTime);
        }
    }

    private void SmoothSpeedChange()
    {
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedSmoothing * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        splineAnimate.MaxSpeed = currentSpeed;
        
        // Optional: Change animation speed based on velocity
        if (TryGetComponent<Animator>(out var animator))
        {
            animator.SetFloat("SpeedMultiplier", currentSpeed / baseSpeed);
        }
    }

    // Public methods for UI or other scripts
    public void SetBaseSpeed(float newSpeed)
    {
        baseSpeed = newSpeed;
        targetSpeed = newSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
}