using UnityEngine;

public class AnalogGauge : MonoBehaviour
{
    public enum GaugeType { Pressure, Flow, Temperature }

    [Header("References")]
    [SerializeField] private SystemModel system;
    [SerializeField] private GaugeType gaugeType;

    [Header("Value Mapping")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 100f;

    [Header("Needle Rotation (Z axis recommended)")]
    [SerializeField] private float minAngleZ = -60f;  // left side
    [SerializeField] private float maxAngleZ = 60f;   // right side

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f;

    private float currentAngle;

    void Update()
    {
        if (system == null) return;

        float value = gaugeType switch
        {
            GaugeType.Pressure => system.pressure,
            GaugeType.Flow => system.flow,
            GaugeType.Temperature => system.temperature,
            _ => 0f
        };

        float t = Mathf.InverseLerp(minValue, maxValue, value);
        float targetAngle = Mathf.Lerp(minAngleZ, maxAngleZ, t);

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);

        // Rotate around local Z (most 2D needles use Z)
        transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}
