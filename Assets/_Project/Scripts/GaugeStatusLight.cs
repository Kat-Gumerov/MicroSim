using UnityEngine;

public class StatusLight : MonoBehaviour
{
    public enum ParameterType
    {
        Pressure,
        Flow,
        Temperature
    }

    [Header("References")]
    [SerializeField] private SystemModel system;
    [SerializeField] private ParameterType parameter;

    [Header("Colors")]
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color redColor = Color.red;

    [Header("Thresholds")]
    [SerializeField] private float warningMargin = 5f;

    private Renderer rend;
    private Material matInstance;

    [SerializeField] private bool frozen = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            matInstance = rend.material;
        }
    }

    private void Update()
    {
        if (system == null || matInstance == null) return;
        if (frozen) return;   // keep whatever color it last had

        float value;
        Vector2 range;

        switch (parameter)
        {
            case ParameterType.Pressure:
                value = system.pressure;
                range = system.pressureRange;
                break;

            case ParameterType.Flow:
                value = system.flow;
                range = system.flowRange;
                break;

            case ParameterType.Temperature:
                value = system.temperature;
                range = system.temperatureRange;
                break;

            default:
                return;
        }

        bool isGreen = value >= range.x && value <= range.y;
        bool isYellow = !isGreen &&
                        value >= range.x - warningMargin &&
                        value <= range.y + warningMargin;
        bool isRed = !isGreen && !isYellow;

        Color target = isGreen ? greenColor :
                       isYellow ? yellowColor :
                       redColor;

        matInstance.SetColor("_Color", target);
        if (matInstance.HasProperty("_EmissionColor"))
            matInstance.SetColor("_EmissionColor", target);
    }

    public void SetFrozen(bool value)
    {
        frozen = value;
    }

    public bool IsFrozen => frozen;
}
