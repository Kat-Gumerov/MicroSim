using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;

public class ScenarioManagerB : MonoBehaviour
{
    [Header("Scenario Settings")]
    public float recordIntervalSeconds = 15f;
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

    float scenarioStartTime;
    public float ElapsedTime => scenarioRunning ? Time.time - scenarioStartTime : 0f;

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

    public void OnStartScenarioPressed()
    {
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (scenarioHUD != null) scenarioHUD.SetActive(true);
        if (summaryPanel != null) summaryPanel.SetActive(false);

        scenarioStartTime = Time.time;
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
            system.ResetToNormal();

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

        float t = ElapsedTime;
        string line = $"{t:0.0}s | ";

        bool allGreen = system != null && system.AllInNormalRange();

        foreach (var g in gauges)
        {
            if (g == null) continue;

            float v = g.GetCurrentValue();
            bool green = g.IsInGreenRange();

            line += $"{g.GaugeTypeKind}:{v:0.0}{(green ? "" : "*")} ";
        }

        line += allGreen ? "| OK" : "| CHECK";
        AppendLog(line);

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

        if (recordsDone == 2 && panelFreezeAnomaly != null && !panelFreezeAnomaly.Active)
            panelFreezeAnomaly.Trigger();

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

        // wait a moment so the user can see the final log line
        StartCoroutine(ShowSummaryAfterDelay());
    }

    IEnumerator ShowSummaryAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        ShowSummary();
    }

    void ShowSummary()
    {
        if (summaryPanel == null || summaryText == null) return;

        if (scenarioHUD != null) scenarioHUD.SetActive(false);
        summaryPanel.SetActive(true);

        var sb = new StringBuilder();

        sb.AppendLine("This scenario tested whether the operator could notice small, unexpected changes and stabilize the system without step-by-step instructions.");
        sb.AppendLine();

        if (hadRecord1)
            sb.AppendLine($"- Record 1 ({record1Time:0.0}s): " +
                          (record1AllGreen ? "All indicators within normal range." :
                                             "One or more indicators were outside the normal range."));

        if (hadRecord2)
            sb.AppendLine($"- Record 2 ({record2Time:0.0}s): " +
                          (record2AllGreen ? "System recorded in a normal range." :
                                             "System still showed abnormal values."));

        if (hadRecord3)
            sb.AppendLine($"- Record 3 ({record3Time:0.0}s): " +
                          (record3AllGreen ? "System recorded in a stable range at the end of the scenario." :
                                             "System was not fully stabilized at the end of the scenario."));

        sb.AppendLine();
        sb.AppendLine("Anomaly 1 – Gradual value drift:");

        if (anomalyController != null && anomalyController.pressureAnomalyStartTime >= 0f)
            sb.AppendLine($"- Pressure began drifting away from normal at about {anomalyController.pressureAnomalyStartTime:0.0}s, moving toward a high value ({anomalyController.pressureAnomalyValue:0.0}).");

        if (anomalyController != null && anomalyController.flowAnomalyStartTime >= 0f)
            sb.AppendLine($"- Flow began drifting away from normal at about {anomalyController.flowAnomalyStartTime:0.0}s, moving toward a low value ({anomalyController.flowAnomalyValue:0.0}).");

        if (system != null)
        {
            sb.AppendLine();
            sb.AppendLine("Temperature response:");

            bool tempInRangeEnd = system.TemperatureInRange();

            if (anomalyController != null && anomalyController.pressureAnomalyStartTime >= 0f)
            {
                if (tempInRangeEnd)
                    sb.AppendLine("- Temperature rose along with the pressure drift but was brought back into its normal range by the end of the scenario.");
                else
                    sb.AppendLine("- Temperature rose along with the pressure drift and was not fully returned to its normal range by the end of the scenario.");
            }
            else
            {
                if (tempInRangeEnd)
                    sb.AppendLine("- Temperature remained within its normal range throughout the scenario.");
                else
                    sb.AppendLine("- Temperature moved outside its normal range during the scenario.");
            }
        }

        if (hadRecord2)
        {
            if (record2AllGreen)
                sb.AppendLine("- By the second record, all indicators were back in the normal range.");
            else
                sb.AppendLine("- At the time of the second record, at least one indicator remained out of range.");
        }

        if (panelFreezeAnomaly != null && panelFreezeAnomaly.freezeStartTime >= 0f)
        {
            sb.AppendLine();
            sb.AppendLine("Anomaly 2 – Loss of indication (frozen panel):");
            sb.AppendLine($"- Panel indications froze at approximately {panelFreezeAnomaly.freezeStartTime:0.0}s.");

            if (panelFreezeAnomaly.freezeClearTime >= 0f)
            {
                float responseTime = panelFreezeAnomaly.freezeClearTime - panelFreezeAnomaly.freezeStartTime;
                sb.AppendLine($"- AUTO enabled at approximately {panelFreezeAnomaly.freezeClearTime:0.0}s (response time ≈ {responseTime:0.0}s).");
            }
            else
                sb.AppendLine("- No AUTO-mode stabilization occurred while the panel was frozen.");
        }

        summaryText.text = sb.ToString();
    }

    public void OnExitSummaryPressed()
    {
        if (summaryPanel != null)
            summaryPanel.SetActive(false);

        var focus = FindObjectOfType<PanelFocusManager>();
        if (focus != null)
            focus.EnterPlayerMode();
        else
            Debug.LogWarning("OnExitSummaryPressed: PanelFocusManager not found in scene.");
    }
}
