using UnityEngine;

public class AnalogGauge : MonoBehaviour
{
    public enum GaugeType
    {
        Pressure,
        Flow,
        Temperature,
        PumpSpeed,
        ValvePosition
    }

    [Header("References")]
    [SerializeField] private SystemModel system;
    [SerializeField] private GaugeType gaugeType;

    [Header("Value Mapping")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 50f;

    [Header("Green Range (normal)")]
    [SerializeField] private float greenMin = 10f;
    [SerializeField] private float greenMax = 40f;

    [Header("Needle Rotation (Y axis)")]
    [SerializeField] private float minAngleOffset = -20f;
    [SerializeField] private float maxAngleOffset = 20f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f;

    private float currentAngleOffset;
    private float baseY;
    private bool initialized = false;

    // runtime value we expose to other scripts
    private float currentValue;

    private void Awake()
    {
        baseY = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (system == null) return;

        // read from SystemModel
        float rawValue = gaugeType switch
        {
            GaugeType.Pressure => system.pressure,
            GaugeType.Flow => system.flow,
            GaugeType.Temperature => system.temperature,
            GaugeType.PumpSpeed => system.pumpSpeed,
            GaugeType.ValvePosition => system.valvePosition,
            _ => 0f
        };

        currentValue = rawValue;

        // map to needle rotation
        float t = Mathf.InverseLerp(minValue, maxValue, rawValue);
        float targetOffset = Mathf.Lerp(minAngleOffset, maxAngleOffset, t);

        if (!initialized)
        {
            currentAngleOffset = targetOffset;
            initialized = true;
        }
        else
        {
            currentAngleOffset = Mathf.Lerp(
                currentAngleOffset,
                targetOffset,
                smoothSpeed * Time.deltaTime
            );
        }

        Vector3 e = transform.localEulerAngles;
        e.y = baseY + currentAngleOffset;
        transform.localEulerAngles = e;
    }

    // ===== API used by Scenario B =====

    public float GetCurrentValue()
    {
        return currentValue;
    }

    public bool IsInGreenRange()
    {
        return currentValue >= greenMin && currentValue <= greenMax;
    }

    // public read-only access to the enum for logging
    public GaugeType GaugeTypeKind => gaugeType;
}
