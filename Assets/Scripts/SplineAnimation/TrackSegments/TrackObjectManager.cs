using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

public class TrackObjectManager : MonoBehaviour
{
    public SplineContainer splineContainer;
    public TrackSegment[] trackSegments;

    public TrackSegment GetActiveSegment(float normalizedPos)
    {
        foreach (var segment in trackSegments)
        {
            if (segment.IsActive(normalizedPos))
            {
                return segment;
            }
        }
        return null;
    }

    // Visualize segments in the editor
    private void OnDrawGizmosSelected()
    {
        if (splineContainer == null || trackSegments == null) return;
        
        foreach (var segment in trackSegments)
        {
            if (segment.start > 1 || segment.end > 1) continue;
            
            Vector3 startPos = splineContainer.EvaluatePosition(segment.start);
            Vector3 endPos = splineContainer.EvaluatePosition(segment.end);
            
            // Set color based on type
            Color segmentColor = Color.white;
            switch (segment.type)
            {
                case TrackObjectType.Lift: segmentColor = Color.green; break;
                case TrackObjectType.Brake: segmentColor = Color.red; break;
                case TrackObjectType.Booster: segmentColor = Color.blue; break;
                case TrackObjectType.Checkpoint: segmentColor = Color.yellow; break;
            }
            
            Gizmos.color = segmentColor;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(startPos, 0.2f);
            Gizmos.DrawSphere(endPos, 0.2f);
            
            // Draw segment type label
            GUIStyle style = new GUIStyle();
            style.normal.textColor = segmentColor;
            Handles.Label(Vector3.Lerp(startPos, endPos, 0.5f), segment.type.ToString(), style);
        }
    }
}