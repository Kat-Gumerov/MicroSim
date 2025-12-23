using UnityEngine;

public class SystemModel : MonoBehaviour
{
    [Header("True System Values")]
    public float pressure = 50f;
    public float temperature = 70f;
    public float flow = 25f;

    [Header("Normal (Green) Ranges")]
    public Vector2 pressureRange = new Vector2(40f, 60f);
    public Vector2 temperatureRange = new Vector2(65f, 75f);
    public Vector2 flowRange = new Vector2(20f, 30f);

    [Header("Baseline Drift (Normal Operation)")]
    public float pressureDrift = 0.3f;
    public float temperatureDrift = 0.2f;
    public float flowDrift = 0.25f;

    [Header("Operator Controls (0–1)")]
    [Range(0f, 1f)] public float pumpSpeed = 0.5f;
    [Range(0f, 1f)] public float valvePosition = 0.5f;

    [Header("System Mode")]
    public bool manualMode = false;

    void Update()
    {
        float dt = Time.deltaTime;

        // --- Natural system drift (system is always "alive") ---
        pressure += pressureDrift * dt;
        temperature += temperatureDrift * dt;
        flow += flowDrift * dt;

        // --- Control influence ---
        // Pump speed increases flow and pressure
        flow += Mathf.Lerp(-0.4f, 0.6f, pumpSpeed) * dt;
        pressure += Mathf.Lerp(-0.3f, 0.5f, pumpSpeed) * dt;

        // Valve position relieves pressure but can affect flow
        flow += Mathf.Lerp(-0.2f, 0.5f, valvePosition) * dt;
        pressure += Mathf.Lerp(0.3f, -0.4f, valvePosition) * dt;

        // --- Automatic stabilization (AUTO mode only) ---
        if (!manualMode)
        {
            pressure = Mathf.Lerp(
                pressure,
                (pressureRange.x + pressureRange.y) * 0.5f,
                0.2f * dt
            );

            temperature = Mathf.Lerp(
                temperature,
                (temperatureRange.x + temperatureRange.y) * 0.5f,
                0.1f * dt
            );

            flow = Mathf.Lerp(
                flow,
                (flowRange.x + flowRange.y) * 0.5f,
                0.2f * dt
            );
        }

        // --- Safety clamps (prototype guardrails) ---
        pressure = Mathf.Clamp(pressure, 0f, 100f);
        temperature = Mathf.Clamp(temperature, 0f, 100f);
        flow = Mathf.Clamp(flow, 0f, 100f);
    }

    // --- Helper methods for UI / scoring / guidance ---
    public bool PressureInRange()
    {
        return pressure >= pressureRange.x && pressure <= pressureRange.y;
    }

    public bool TemperatureInRange()
    {
        return temperature >= temperatureRange.x && temperature <= temperatureRange.y;
    }

    public bool FlowInRange()
    {
        return flow >= flowRange.x && flow <= flowRange.y;
    }

    public bool AllInNormalRange()
    {
        return PressureInRange() &&
               TemperatureInRange() &&
               FlowInRange();
    }
}
