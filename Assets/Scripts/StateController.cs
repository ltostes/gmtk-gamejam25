using UnityEngine;
using UnityEngine.InputSystem;

public class StateController : MonoBehaviour
{

    [SerializeField] public CustomSplineAnimator customSplineAnimator;

    private PassengerSpawner passengerSpawner;

    void Start()
    {
        passengerSpawner = GetComponent<PassengerSpawner>();
    }

    [ContextMenu("Reset State")]
    public void resetState()
    {
        customSplineAnimator.resetPosition();
        passengerSpawner.passengerReset();
    }

}
