using UnityEngine;

public enum TrackObjectType { Lift, Brake, Booster, Checkpoint }

[System.Serializable]
public class TrackSegment
{
    public float start = 0.1f;
    public float end = 0.15f;
    public TrackObjectType type;
    
    [Header("Type Parameters")]
    public float liftSpeed = 5f;           // For Lift
    public float brakeStrength = 2f;        // For Brake
    public float brakeMinSpeed = 1f;           // For Lift
    public float boosterForce = 10f;        // For Booster
    
    public bool IsActive(float normalizedPos)
    {
        return normalizedPos >= start && normalizedPos <= end;
    }
}