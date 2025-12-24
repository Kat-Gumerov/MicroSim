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

    [Header("Needle Rotation (Y axis)")]
    [SerializeField] private float minAngleOffset = -20f;
    [SerializeField] private float maxAngleOffset = 20f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f;

    private float currentAngleOffset;
    private float baseY;
    private bool initialized = false;

    private void Awake()
    {
        baseY = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (system == null) return;

        float rawValue = gaugeType switch
        {
            GaugeType.Pressure => system.pressure,
            GaugeType.Flow => system.flow,
            GaugeType.Temperature => system.temperature,
            GaugeType.PumpSpeed => system.pumpSpeed,
            GaugeType.ValvePosition => system.valvePosition,
            _ => 0f
        };

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
}
