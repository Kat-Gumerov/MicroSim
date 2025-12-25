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

    [Header("Panel UI")]
    [SerializeField] private GameObject panelUI;   // <-- drag PanelUI here in Inspector

    public bool InPanelMode { get; private set; }

    void Start()
    {
        // make sure UI is hidden at start
        if (panelUI != null)
            panelUI.SetActive(false);

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

        // show the panel UI
        if (panelUI != null)
            panelUI.SetActive(true);

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

        // hide the panel UI
        if (panelUI != null)
            panelUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
