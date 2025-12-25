using UnityEngine;

public class RecordButton : MonoBehaviour, IInteractable
{
    public ScenarioManagerB scenarioManager;

    public void Interact()
    {
        if (scenarioManager != null)
            scenarioManager.OnRecordButtonPressed();
    }
}
