using UnityEngine;

public class InputVisualizer : MonoBehaviour
{
    public CustomSplineAnimator animator;
    public float guiOffsetY = 20f;
    public float guiWidth = 200f;
    public float guiHeight = 80f;

    void OnGUI()
    {
        if (animator == null) return;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 16;
        boxStyle.normal.textColor = Color.white;

        Rect rect = new Rect(10, guiOffsetY, guiWidth, guiHeight);
        string info = $"Speed: {animator.currentSpeed:F1}\n" +
                     $"Accel: {animator._currentAcceleration:F1}\n" +
                     $"Brake: {animator._currentBraking:F1}";

        GUI.Box(rect, info, boxStyle);
    }
}