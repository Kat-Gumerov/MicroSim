using UnityEngine;

public class ModeToggle : MonoBehaviour, IInteractable
{
    [SerializeField] private SystemModel system;

    public void Interact()
    {
        system.manualMode = !system.manualMode;

        Debug.Log(system.manualMode ? "MANUAL MODE" : "AUTO MODE");
        Debug.Log("manualMode: " + system.manualMode);
    }
}
