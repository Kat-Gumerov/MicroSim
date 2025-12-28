using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private PanelFocusManager focusManager;
    [SerializeField] private Camera panelCamera;   // drag PanelCamera here
    [SerializeField] private LayerMask interactMask = ~0;

    Camera playerCam;

    void Awake()
    {
        playerCam = GetComponent<Camera>();
    }

    void Update()
    {
        // Pick which camera to use (player vs panel)
        Camera camToUse = (focusManager != null && focusManager.InPanelMode && panelCamera != null)
            ? panelCamera
            : playerCam;

        Ray ray = camToUse.ScreenPointToRay(Input.mousePosition);

        // Do a raycast every frame for hover + click
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            // ---------- HOVER HINT ----------
            string hintText = null;

            // KnobControl has a public string hint
            if (hit.collider.TryGetComponent(out KnobControl knob))
            {
                hintText = knob.hint;
            }
            else if (hit.collider.GetComponentInParent<KnobControl>() is KnobControl parentKnob)
            {
                hintText = parentKnob.hint;
            }

            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Show(hintText);

            // ---------- CLICK INTERACTION ----------
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Clicked: " + hit.collider.gameObject.name);

                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null)
                    interactable.Interact();
            }
        }
        else
        {
            // Nothing under the cursor → hide tooltip
            if (TooltipUI.Instance != null)
                TooltipUI.Instance.Show(null);
        }
    }
}
