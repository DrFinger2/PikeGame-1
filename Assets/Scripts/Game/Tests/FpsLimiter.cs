using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [Header("Framerate Settings")]
    [Tooltip("Set this to 30, 60, 120, etc. Change it while playing to feel the difference!")]
    public int targetFPS = 60;

    void Start()
    {
        // 1. VSync MUST be 0, otherwise the monitor's refresh rate takes over
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = targetFPS;
    }

    void Update()
    {
        // If you change the value in the Inspector while playing, apply it immediately
        if (Application.targetFrameRate != targetFPS)
        {
            Application.targetFrameRate = targetFPS;
        }
    }
}