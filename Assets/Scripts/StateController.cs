using UnityEngine;
using UnityEngine.InputSystem;

public class StateController : MonoBehaviour
{

    [SerializeField] public CustomSplineAnimator customSplineAnimator;

    private PassengerSpawner passengerSpawner;

    [Header("State Variables")]
    public bool isActive;
    public bool isStarted;
    public int livePassengers;

    private float elapsedTime;
    private int elapsedLaps;

    [Header("GUI Settings")]
    public float guiOffsetY = 20f;
    public float guiWidth = 200f;
    public float guiHeight = 100f;

    void Start()
    {
        passengerSpawner = GetComponent<PassengerSpawner>();
        isActive = true;
        isStarted = false;
    }

    [ContextMenu("Reset State")]
    public void resetState()
    {
        isStarted = false;
        elapsedTime = 0f;
        elapsedLaps = 0;
        customSplineAnimator.resetPosition();
        passengerSpawner.passengerReset();
    }

    void Update()
    {
        livePassengers = passengerSpawner.getLivePassengers();
        isActive = livePassengers > 0;
        if (isActive && isStarted)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    [ContextMenu("Add Lap")]
    public void addLap()
    {
        if (isActive && isStarted)
        {
            elapsedLaps += 1;
        }
    }

    void OnGUI()
    {
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.normal.textColor = Color.white;

        Rect rect = new Rect(10, guiOffsetY, guiWidth, guiHeight);
        string info = $"Live Riders: {livePassengers:F0}\n" +
                      $"Active/Started: {isActive} / {isStarted}\n" +
                      $"Elapsed Time: {elapsedTime:F1}\n" +
                      $"Elapsed Laps: {elapsedLaps}";

        GUI.Box(rect, info, boxStyle);
    }
}
