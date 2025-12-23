using UnityEngine;

public class PanelFocusManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera panelCamera;

    [Header("Panel Collider (to disable in panel mode)")]
    [SerializeField] private Collider panelStandCollider;

    [Header("Optional: disable player controls")]
    [SerializeField] private MonoBehaviour[] disableWhileInPanelMode;

    public bool InPanelMode { get; private set; }

    void Start()
    {
        EnterPlayerMode();
    }

    void Update()
    {
        if (InPanelMode && Input.GetKeyDown(KeyCode.Escape))
        {
            EnterPlayerMode();
        }
    }

    public void EnterPanelMode()
    {
        Debug.Log("Entering PANEL MODE");

        InPanelMode = true;

        // Disable panel stand collider so it can't steal clicks
        if (panelStandCollider != null)
            panelStandCollider.enabled = false;

        playerCamera.enabled = false;
        panelCamera.enabled = true;

        foreach (var b in disableWhileInPanelMode)
            if (b != null) b.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EnterPlayerMode()
    {
        Debug.Log("Entering PLAYER MODE");

        InPanelMode = false;

        // Re-enable panel stand collider
        if (panelStandCollider != null)
            panelStandCollider.enabled = true;

        panelCamera.enabled = false;
        playerCamera.enabled = true;

        foreach (var b in disableWhileInPanelMode)
            if (b != null) b.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
