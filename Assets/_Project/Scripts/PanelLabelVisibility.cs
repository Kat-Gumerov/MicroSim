using UnityEngine;

public class PanelLabelVisibility : MonoBehaviour
{
    [SerializeField] private PanelFocusManager focusManager;
    [SerializeField] private ScenarioManagerB scenarioManager;

    void LateUpdate()
    {
        bool show =
            focusManager != null &&
            focusManager.InPanelMode &&
            scenarioManager != null &&
            scenarioManager.ScenarioRunning;

        foreach (Transform child in transform)
            child.gameObject.SetActive(show);
    }
}

