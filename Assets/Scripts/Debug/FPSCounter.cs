using UnityEngine;
using System.Collections.Generic;

public class FPSCounter : MonoBehaviour
{
    public float updateInterval = 0.5f; // Co ile sekund odœwie¿aæ aktualne FPS

    private float currentFPS;
    private float minFPS = float.MaxValue;
    private float maxFPS = 0f;
    private float averageFPS;

    private float timePassed = 0f;
    private int frameCount = 0;

    private float totalFPS = 0f;
    private int totalFrameSamples = 0;

    void Update()
    {
        float fps = 1f / Time.unscaledDeltaTime;

        // Aktualne FPS (odœwie¿ane co interval)
        timePassed += Time.unscaledDeltaTime;
        frameCount++;

        if (timePassed >= updateInterval)
        {
            currentFPS = frameCount / timePassed;
            frameCount = 0;
            timePassed = 0f;
        }

        // Min / Max
        if (fps < minFPS) minFPS = fps;
        if (fps > maxFPS) maxFPS = fps;

        // Œrednia
        totalFPS += fps;
        totalFrameSamples++;
        averageFPS = totalFPS / totalFrameSamples;
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150));

        GUILayout.Label("FPS Monitor", style);
        GUILayout.Label($"Current FPS: {currentFPS:F1}", style);
        GUILayout.Label($"Min FPS: {minFPS:F1}", style);
        GUILayout.Label($"Max FPS: {maxFPS:F1}", style);
        GUILayout.Label($"Average FPS: {averageFPS:F1}", style);

        GUILayout.EndArea();
    }
}