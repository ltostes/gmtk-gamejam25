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
    private float elapsedLapTime;
    private int elapsedLaps;
    private float currentScore;
    private float potentialPassengerLapScore;

    [Header("Game Settings")]
    public int referenceLapTime = 60;

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
        elapsedLapTime = 0f;
        elapsedLaps = 0;
        currentScore = 0;
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
            elapsedLapTime += Time.deltaTime;
        }
        potentialPassengerLapScore = referenceLapTime - elapsedLapTime;
    }

    [ContextMenu("Add Lap")]
    public void addLap()
    {
        if (isActive && isStarted)
        {
            elapsedLaps += 1;
            elapsedLapTime = 0f;
            currentScore += potentialPassengerLapScore * livePassengers;
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

        Rect score_rect = new Rect(800, guiOffsetY, guiWidth, guiHeight);
        string score_info = $"Current Score: {currentScore:F0}\n" +
                      $"Elapsed LapTime: {elapsedLapTime:F1}\n" +
                      $"P. Lap: {potentialPassengerLapScore:F0} x {livePassengers} = {potentialPassengerLapScore * livePassengers:F1}";

        GUI.Box(rect, info, boxStyle);
        GUI.Box(score_rect, score_info, boxStyle);
    }
}
