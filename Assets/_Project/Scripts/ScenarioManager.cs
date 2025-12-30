using UnityEngine;
using TMPro;
using System.Text;

public class ScenarioManagerB : MonoBehaviour
{
    [Header("Scenario Settings")]
    public float recordIntervalSeconds = 10f;
    public int recordsToComplete = 3;

    [Header("System")]
    public SystemModel system;
    public bool ScenarioRunning => scenarioRunning;

    [Header("UI")]
    public GameObject instructionPanel;
    public GameObject scenarioHUD;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI logText;

    [Header("Summary UI")]
    public GameObject summaryPanel;
    public TextMeshProUGUI summaryText;

    [Header("Gauges")]
    public AnalogGauge[] gauges;

    [Header("Anomalies")]
    public AnomalyController anomalyController;
    public PanelFreezeAnomaly panelFreezeAnomaly;

    float timer;
    bool waitingForRecord;
    int recordsDone;
    bool scenarioRunning;

    // record tracking for summary
    float record1Time, record2Time, record3Time;
    bool record1AllGreen, record2AllGreen, record3AllGreen;
    bool hadRecord1, hadRecord2, hadRecord3;

    void Start()
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (scenarioHUD != null) scenarioHUD.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);

        scenarioRunning = false;
        waitingForRecord = false;
        recordsDone = 0;

        if (logText != null) logText.text = "";
        if (timerText != null) timerText.text = "";
    }

    // Called by Start button
    public void OnStartScenarioPressed()
    {
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (scenarioHUD != null) scenarioHUD.SetActive(true);
        if (summaryPanel != null) summaryPanel.SetActive(false);

        StartScenario();
    }

    void StartScenario()
    {
        timer = recordIntervalSeconds;
        waitingForRecord = false;
        recordsDone = 0;
        scenarioRunning = true;

        hadRecord1 = hadRecord2 = hadRecord3 = false;

        if (logText != null) logText.text = "";
        UpdateTimerUI();

        if (system != null)
            system.ResetToNormal();        // start from normal/green

        if (panelFreezeAnomaly != null)
        {
            panelFreezeAnomaly.Clear();
            panelFreezeAnomaly.ResetTracking();
        }

        if (anomalyController != null)
        {
            anomalyController.ResetTracking();
            anomalyController.BeginAnomalies();
        }
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

        if (!scenarioRunning)
        {
            timerText.text = "Scenario complete";
            return;
        }

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

        bool allGreen = system != null && system.AllInNormalRange();

        foreach (var g in gauges)
        {
            if (g == null) continue;

            float v = g.GetCurrentValue();
            bool green = g.IsInGreenRange();

            // add a * if that specific gauge isn't green
            line += $"{g.GaugeTypeKind}:{v:0.0}{(green ? "" : "*")} ";
        }

        line += allGreen ? "| OK" : "| CHECK";
        AppendLog(line);

        // store info per record (1..3)
        int recordIndex = recordsDone + 1;
        switch (recordIndex)
        {
            case 1:
                record1Time = t;
                record1AllGreen = allGreen;
                hadRecord1 = true;
                break;
            case 2:
                record2Time = t;
                record2AllGreen = allGreen;
                hadRecord2 = true;
                break;
            case 3:
                record3Time = t;
                record3AllGreen = allGreen;
                hadRecord3 = true;
                break;
        }

        recordsDone++;
        waitingForRecord = false;
        timer = recordIntervalSeconds;
        UpdateTimerUI();

        // After the second record, trigger the panel freeze anomaly
        // so the third interval is experienced with a frozen panel.
        if (recordsDone == 2 && panelFreezeAnomaly != null && !panelFreezeAnomaly.Active)
        {
            panelFreezeAnomaly.Trigger();
        }

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
        UpdateTimerUI();

        ShowSummary();
    }

    void ShowSummary()
    {
        if (summaryPanel == null || summaryText == null) return;

        if (scenarioHUD != null) scenarioHUD.SetActive(false);
        summaryPanel.SetActive(true);

        var sb = new StringBuilder();

        sb.AppendLine("Scenario Summary");
        sb.AppendLine();
        sb.AppendLine("This scenario tested whether the operator could notice small, unexpected changes and stabilize the system without step-by-step instructions.");
        sb.AppendLine();

        // --- Record overview ---
        if (hadRecord1)
            sb.AppendLine($"Record 1 ({record1Time:0.0}s): " +
                          (record1AllGreen ? "All indicators within normal range." :
                                             "One or more indicators were outside the normal range."));

        if (hadRecord2)
            sb.AppendLine($"Record 2 ({record2Time:0.0}s): " +
                          (record2AllGreen ? "System recorded in a normal range." :
                                             "System still showed abnormal values."));

        if (hadRecord3)
            sb.AppendLine($"Record 3 ({record3Time:0.0}s): " +
                          (record3AllGreen ? "System recorded in a stable range at the end of the scenario." :
                                             "System was not fully stabilized at the end of the scenario."));

        // --- Anomaly 1: Gradual drift (pressure + flow) ---
        sb.AppendLine();
        sb.AppendLine("Anomaly 1 – Gradual value drift:");

        if (anomalyController != null && anomalyController.pressureAnomalyStartTime >= 0f)
        {
            sb.AppendLine($"- Pressure began drifting away from normal at about {anomalyController.pressureAnomalyStartTime:0.0}s, moving toward a high value ({anomalyController.pressureAnomalyValue:0.0}).");
        }

        if (anomalyController != null && anomalyController.flowAnomalyStartTime >= 0f)
        {
            sb.AppendLine($"- Flow began drifting away from normal at about {anomalyController.flowAnomalyStartTime:0.0}s, moving toward a low value ({anomalyController.flowAnomalyValue:0.0}).");
        }

        if (hadRecord2)
        {
            if (record2AllGreen)
            {
                sb.AppendLine("- By the second record, all indicators were back in the normal range. This suggests the drift was noticed and corrected before it developed into a larger problem.");
            }
            else
            {
                sb.AppendLine("- At the time of the second record, at least one indicator remained out of range. This suggests the gradual drift was not fully recognized or corrected yet.");
            }
        }
        else
        {
            sb.AppendLine("- Not enough data was recorded to evaluate the operator’s response to the drift.");
        }

        // --- Anomaly 2: Frozen panel ---
        if (panelFreezeAnomaly != null && panelFreezeAnomaly.freezeStartTime >= 0f)
        {
            sb.AppendLine();
            sb.AppendLine("Anomaly 2 – Loss of indication (frozen panel):");
            sb.AppendLine($"- Panel indications froze at approximately {panelFreezeAnomaly.freezeStartTime:0.0}s.");

            if (panelFreezeAnomaly.freezeClearTime >= 0f)
            {
                float responseTime = panelFreezeAnomaly.freezeClearTime - panelFreezeAnomaly.freezeStartTime;
                sb.AppendLine($"- The operator switched to AUTO at approximately {panelFreezeAnomaly.freezeClearTime:0.0}s (response time ≈ {responseTime:0.0}s).");
                sb.AppendLine("- Switching to AUTO is a conservative action that restabilizes the system when the panel stops providing reliable feedback, showing that the operator expected something unexpected and moved the system to a safe state.");
            }
            else
            {
                sb.AppendLine("- No AUTO-mode stabilization was applied while the panel was frozen, suggesting the loss of indication may not have been fully recognized.");
            }
        }

        summaryText.text = sb.ToString();
    }
}
