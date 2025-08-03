using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StateController : MonoBehaviour
{

    [SerializeField] public CustomSplineAnimator customSplineAnimator;

    private PassengerManager passengerManager;

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
    [SerializeField] public GameObject startScreen;
    [SerializeField] public GameObject finalScreen;
    [SerializeField] public GameObject scoreCounterBox;
    [SerializeField] public TextMeshProUGUI finalScoreText;
    [SerializeField] public TextMeshProUGUI currentScoreText;
    [SerializeField] public TextMeshProUGUI nextLapScorePerPassengerText;
    [SerializeField] public TextMeshProUGUI passengerMultiplierText;

    void Start()
    {
        passengerManager = GetComponent<PassengerManager>();
        isActive = true;
        isStarted = false;
        resetState();
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
        passengerManager.passengerReset();
    }

    void Update()
    {
        // Passenger dying sound
        int newLivePassengers = passengerManager.getLivePassengers();
        int deltaPassengers = livePassengers - newLivePassengers;

        if (deltaPassengers == 1)
        {
            playPassengerDeathAudio();
        }
        else if (deltaPassengers > 1)
        {
            for (int i = 0; i < deltaPassengers; i++)
            {
                Invoke("playPassengerDeathAudio", i / 10f);
            }
        }

        livePassengers = passengerManager.getLivePassengers();
        isActive = livePassengers > 0;
        if (isActive && isStarted)
        {
            elapsedTime += Time.deltaTime;
            elapsedLapTime += Time.deltaTime;
        }
        potentialPassengerLapScore = referenceLapTime - elapsedLapTime;

        if (potentialPassengerLapScore < 0)
        {
            elapsedLapTime = 0f;
            passengerManager.killAPassenger();
        }

        UpdateGUI();
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

    public void playPassengerDeathAudio()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(passengerManager.passengerDeathAudioClips, passengerManager.cartAnchor.transform, 1);
    }

    void UpdateGUI()
    {
        // Logo
        startScreen.SetActive(isActive && (!isStarted));

        // Final Screen Background
        finalScreen.SetActive(!isActive);

        // Score
        finalScoreText.text = $"{currentScore:F0}";
        currentScoreText.text = $"{currentScore:F0}";
        nextLapScorePerPassengerText.text = $"{potentialPassengerLapScore:F0}";
        passengerMultiplierText.text = $"{livePassengers:F0}";

        scoreCounterBox.SetActive(isActive && isStarted);

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

        // GUI.Box(rect, info, boxStyle);
        // GUI.Box(score_rect, score_info, boxStyle);
    }
}
