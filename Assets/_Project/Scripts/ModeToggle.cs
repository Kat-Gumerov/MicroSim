using UnityEngine;

public class ModeToggle : MonoBehaviour, IInteractable
{
    [SerializeField] private SystemModel system;
    [SerializeField] private PanelFreezeAnomaly panelFreezeAnomaly;

    public void Interact()
    {
        if (system == null) return;

        system.manualMode = !system.manualMode;

        Debug.Log(system.manualMode ? "MANUAL MODE" : "AUTO MODE");
        Debug.Log("manualMode: " + system.manualMode);

        // notify the freeze anomaly so it can clear / log if needed
        if (panelFreezeAnomaly != null)
        {
            panelFreezeAnomaly.OnModeChanged(system.manualMode);
        }
    }
}
