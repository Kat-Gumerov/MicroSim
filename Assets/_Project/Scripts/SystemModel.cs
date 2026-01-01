using UnityEngine;

public class SystemModel : MonoBehaviour
{
    [Header("True System Values")]
    public float pressure;
    public float temperature;
    public float flow;

    [Header("Normal (Green) Ranges")]
    public Vector2 pressureRange = new Vector2(20f, 30f);
    public Vector2 temperatureRange = new Vector2(25f, 35f);
    public Vector2 flowRange = new Vector2(20f, 30f);

    [Header("Operator Controls (0–1)")]
    [Range(0f, 1f)] public float pumpSpeed = 0.5f;
    [Range(0f, 1f)] public float valvePosition = 0.5f;

    [Header("System Mode")]
    public bool manualMode = true;

    [Header("Anomaly Overrides")]
    public bool lockPressure;
    public bool lockFlow;

    float prevPumpSpeed;
    float prevValvePosition;

    void Awake()
    {
        ResetToNormal();
    }

    public void ResetToNormal()
    {
        pumpSpeed = 0.5f;
        valvePosition = 0.5f;

        pressure = Mid(pressureRange);
        temperature = Mid(temperatureRange);
        flow = Mid(flowRange);

        lockPressure = false;
        lockFlow = false;

        prevPumpSpeed = pumpSpeed;
        prevValvePosition = valvePosition;
    }

    float Mid(Vector2 range)
    {
        return (range.x + range.y) * 0.5f;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // if operator moves controls while an anomaly is locked, unlock it
        if (lockPressure || lockFlow)
        {
            float pumpDelta = Mathf.Abs(pumpSpeed - prevPumpSpeed);
            float valveDelta = Mathf.Abs(valvePosition - prevValvePosition);
            if (pumpDelta > 0.05f || valveDelta > 0.05f)
            {
                lockPressure = false;
                lockFlow = false;
            }
        }

        prevPumpSpeed = pumpSpeed;
        prevValvePosition = valvePosition;

        float baseFlow = Mathf.Lerp(5f, 45f, valvePosition);
        float pumpFlowBoost = Mathf.Lerp(-2f, 5f, pumpSpeed);
        float targetFlow = baseFlow + pumpFlowBoost;

        float pumpPressure = Mathf.Lerp(10f, 48f, pumpSpeed);
        float reliefFactor = Mathf.Lerp(1.0f, 0.5f, valvePosition);
        float targetPressure = pumpPressure * reliefFactor;

        float pumpHeat = Mathf.Lerp(20f, 48f, pumpSpeed);
        float cooling = Mathf.Lerp(0f, 8f, valvePosition);
        float targetTemp = pumpHeat - cooling;

        if (!lockPressure)
            pressure = Mathf.Lerp(pressure, targetPressure, 3f * dt);

        if (!lockFlow)
            flow = Mathf.Lerp(flow, targetFlow, 3f * dt);

        temperature = Mathf.Lerp(temperature, targetTemp, 3f * dt);

        if (!manualMode)
        {
            float safeP = Mid(pressureRange);
            float safeT = Mid(temperatureRange);
            float safeF = Mid(flowRange);

            if (!lockPressure)
                pressure = Mathf.Lerp(pressure, safeP, 0.5f * dt);

            if (!lockFlow)
                flow = Mathf.Lerp(flow, safeF, 0.5f * dt);

            temperature = Mathf.Lerp(temperature, safeT, 0.3f * dt);
        }

        pressure = Mathf.Clamp(pressure, 0f, 50f);
        temperature = Mathf.Clamp(temperature, 0f, 50f);
        flow = Mathf.Clamp(flow, 0f, 50f);
    }

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
