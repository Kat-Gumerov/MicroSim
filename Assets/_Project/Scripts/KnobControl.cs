using UnityEngine;

public class KnobControl : MonoBehaviour, IInteractable
{
    public enum KnobType
    {
        PumpSpeed,
        ValvePosition
    }

    [Header("References")]
    [SerializeField] private SystemModel system;

    [Header("Knob Function")]
    [SerializeField] private KnobType knobType;

    [Header("Interaction")]
    [SerializeField] private float step = 0.1f;

    public void Interact()
    {
        // Click = increase, Shift+Click = decrease
        float direction = Input.GetKey(KeyCode.LeftShift) ? -1f : 1f;

        switch (knobType)
        {
            case KnobType.PumpSpeed:
                system.pumpSpeed = Mathf.Clamp01(system.pumpSpeed + direction * step);
                Debug.Log($"PumpSpeed now: {system.pumpSpeed:F2}  ValvePos now: {system.valvePosition:F2}");
                break;

            case KnobType.ValvePosition:
                system.valvePosition = Mathf.Clamp01(system.valvePosition + direction * step);
                Debug.Log($"PumpSpeed now: {system.pumpSpeed:F2}  ValvePos now: {system.valvePosition:F2}");
                break;
        }
    }
}
