using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private PanelFocusManager focusManager;
    [SerializeField] private Camera panelCamera;   // drag PanelCamera here
    [SerializeField] private LayerMask interactMask = ~0;

    private Camera playerCam;

    void Awake()
    {
        playerCam = GetComponent<Camera>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Camera camToUse = (focusManager != null && focusManager.InPanelMode && panelCamera != null)
            ? panelCamera
            : playerCam;

        Ray ray = camToUse.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            Debug.Log("Clicked: " + hit.collider.gameObject.name);

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null) interactable.Interact();
        }
    }
}
