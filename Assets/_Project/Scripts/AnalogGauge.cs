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

    private float currentValue;

    // freeze support
    [SerializeField] private bool frozen = false;
    private float frozenValue;

    private void Awake()
    {
        baseY = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if (system == null) return;

        float rawValue;

        if (frozen)
        {
            rawValue = frozenValue;
        }
        else
        {
            rawValue = gaugeType switch
            {
                GaugeType.Pressure => system.pressure,
                GaugeType.Flow => system.flow,
                GaugeType.Temperature => system.temperature,
                GaugeType.PumpSpeed => system.pumpSpeed,
                GaugeType.ValvePosition => system.valvePosition,
                _ => 0f
            };

            frozenValue = rawValue;
        }

        currentValue = rawValue;

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

    public float GetCurrentValue()
    {
        return currentValue;
    }

    public bool IsInGreenRange()
    {
        return currentValue >= greenMin && currentValue <= greenMax;
    }

    public GaugeType GaugeTypeKind => gaugeType;

    public void SetFrozen(bool value)
    {
        if (value && !frozen)
        {
            frozenValue = currentValue;
        }

        frozen = value;
    }

    public bool IsFrozen => frozen;
}
