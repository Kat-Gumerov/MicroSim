using UnityEngine;
using TMPro;

public class ScenarioManagerB : MonoBehaviour
{
    [Header("Scenario Settings")]
    public float recordIntervalSeconds = 10f;
    public int recordsToComplete = 5;

    [Header("UI")]
    public GameObject instructionPanel;  // <-- new
    public GameObject scenarioHUD;       // <-- new (Timer + Log + Record)
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI logText;

    [Header("Gauges")]
    public AnalogGauge[] gauges;

    [Header("Anomalies")]
    public AnomalyController anomalyController;

    float timer;
    bool waitingForRecord;
    int recordsDone;
    bool scenarioRunning;

    void Start()
    {
        // show instructions first, hide HUD
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (scenarioHUD != null) scenarioHUD.SetActive(false);

        scenarioRunning = false;
        waitingForRecord = false;

        if (logText != null) logText.text = "";
        if (timerText != null) timerText.text = "";
    }

    // called by StartButton
    public void OnStartScenarioPressed()
    {
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (scenarioHUD != null) scenarioHUD.SetActive(true);

        StartScenario();
    }

    void StartScenario()
    {
        timer = recordIntervalSeconds;
        waitingForRecord = false;
        recordsDone = 0;
        scenarioRunning = true;

        if (logText != null) logText.text = "";
        UpdateTimerUI();

        if (anomalyController != null)
            anomalyController.BeginAnomalies();
    }

    void Update()
    {
        if (!scenarioRunning || waitingForRecord) return;

        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;

        UpdateTimerUI();

        if (timer <= 0f)
        {
            waitingForRecord = true;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        if (waitingForRecord)
            timerText.text = "Press RECORD";
        else
            timerText.text = $"Next record in: {timer:0.0}s";
    }

    public void OnRecordButtonPressed()
    {
        if (!scenarioRunning || !waitingForRecord) return;

        float t = Time.timeSinceLevelLoad;

        string line = $"{t:0.0}s | ";
        bool allGreen = true;

        foreach (var g in gauges)
        {
            if (g == null) continue;

            float v = g.GetCurrentValue();
            bool green = g.IsInGreenRange();
            if (!green) allGreen = false;

            line += $"{g.GaugeTypeKind}:{v:0.0} ";
        }

        line += allGreen ? "| OK" : "| CHECK";
        AppendLog(line);

        recordsDone++;
        waitingForRecord = false;
        timer = recordIntervalSeconds;
        UpdateTimerUI();

        if (recordsDone >= recordsToComplete)
            EndScenario();
    }

    void AppendLog(string line)
    {
        if (logText == null) return;

        if (string.IsNullOrEmpty(logText.text))
            logText.text = line;
        else
            logText.text += "\n" + line;
    }

    void EndScenario()
    {
        scenarioRunning = false;
        AppendLog("=== Scenario complete ===");
        if (timerText != null) timerText.text = "Scenario complete";
    }
}
