#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackObjectManager))]
public class TrackObjectManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        TrackObjectManager manager = (TrackObjectManager)target;
        
        GUILayout.Space(10);
        if (GUILayout.Button("Add New Segment"))
        {
            AddNewSegment(manager);
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("Auto-Space Segments"))
        {
            AutoSpaceSegments(manager);
        }
    }
    
    private void AddNewSegment(TrackObjectManager manager)
    {
        int newSize = manager.trackSegments.Length + 1;
        System.Array.Resize(ref manager.trackSegments, newSize);
        
        if (newSize > 1)
        {
            TrackSegment last = manager.trackSegments[newSize - 2];
            manager.trackSegments[newSize - 1] = new TrackSegment {
                start = Mathf.Clamp01(last.end + 0.05f),
                end = Mathf.Clamp01(last.end + 0.1f),
                type = TrackObjectType.Lift
            };
        }
        else
        {
            manager.trackSegments[0] = new TrackSegment {
                start = 0.1f,
                end = 0.15f,
                type = TrackObjectType.Lift
            };
        }
    }
    
    private void AutoSpaceSegments(TrackObjectManager manager)
    {
        if (manager.trackSegments.Length == 0) return;
        
        float segmentLength = 1f / manager.trackSegments.Length;
        
        for (int i = 0; i < manager.trackSegments.Length; i++)
        {
            manager.trackSegments[i].start = i * segmentLength;
            manager.trackSegments[i].end = (i + 1) * segmentLength - 0.05f;
        }
    }
}
#endif