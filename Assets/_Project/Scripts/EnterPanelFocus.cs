using UnityEngine;

public class EnterPanelFocus : MonoBehaviour, IInteractable
{
    [SerializeField] private PanelFocusManager focusManager;

    public void Interact()
    {
        if (focusManager == null)
        {
            Debug.LogError("PanelFocusManager not assigned!");
            return;
        }

        // If we're already in panel mode, ignore clicks on the panel surface
        if (focusManager.InPanelMode)
            return;

        focusManager.EnterPanelMode();
    }
}
