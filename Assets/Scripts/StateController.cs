using UnityEngine;
using UnityEngine.InputSystem;

public class StateController : MonoBehaviour
{

    [SerializeField] public CustomSplineAnimator customSplineAnimator;

    private PassengerSpawner passengerSpawner;

    [Header("State Variables")]
    public bool isActive;
    public int livePassengers;

    [Header("GUI Settings")]
    public float guiOffsetY = 20f;
    public float guiWidth = 200f;
    public float guiHeight = 80f;

    void Start()
    {
        passengerSpawner = GetComponent<PassengerSpawner>();
        isActive = true;
    }

    [ContextMenu("Reset State")]
    public void resetState()
    {
        customSplineAnimator.resetPosition();
        passengerSpawner.passengerReset();
    }

    void Update()
    {
        livePassengers = passengerSpawner.getLivePassengers();
        isActive = livePassengers > 0;
    }

    void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.normal.textColor = Color.white;

        Rect rect = new Rect(10, guiOffsetY, guiWidth, guiHeight);
        string info = $"Live Riders: {livePassengers:F0}\n" +
                      $"Active: {isActive}";

        GUI.Box(rect, info, boxStyle);
    }
}
