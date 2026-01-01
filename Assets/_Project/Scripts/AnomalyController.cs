using UnityEngine;
using System.Collections;

public class AnomalyController : MonoBehaviour
{
    [Header("References")]
    public SystemModel system;
    public ScenarioManagerB scenarioManager;

    [Header("Anomaly Timing (seconds from scenario start)")]
    public float pressureAnomalyTime = 15f;
    public float flowAnomalyTime = 30f;

    [Header("Anomaly Targets")]
    public float pressureAnomalyValue = 45f; // high
    public float flowAnomalyValue = 5f;      // low

    [Header("Drift Settings")]
    public float driftDuration = 5f;

    Coroutine routine;

    public float pressureAnomalyStartTime { get; private set; } = -1f;
    public float flowAnomalyStartTime { get; private set; } = -1f;

    float Now()
    {
        return (scenarioManager != null) ? scenarioManager.ElapsedTime : Time.timeSinceLevelLoad;
    }

    public void ResetTracking()
    {
        pressureAnomalyStartTime = -1f;
        flowAnomalyStartTime = -1f;
    }

    public void BeginAnomalies()
    {
        if (routine != null) StopCoroutine(routine);
        ResetTracking();

        if (system != null)
        {
            system.lockPressure = false;
            system.lockFlow = false;
        }

        routine = StartCoroutine(AnomalyRoutine());
    }

    IEnumerator AnomalyRoutine()
    {
        if (system == null) yield break;

        float wait1 = pressureAnomalyTime - Now();
        if (wait1 > 0f) yield return new WaitForSeconds(wait1);

        pressureAnomalyStartTime = Now();

        yield return StartCoroutine(DriftValue(
            () => system.pressure,
            v => system.pressure = v,
            pressureAnomalyValue,
            driftDuration
        ));

        if (system != null)
            system.lockPressure = true;   // keeps pressure “bad” until the operator moves controls

        float wait2 = flowAnomalyTime - Now();
        if (wait2 > 0f) yield return new WaitForSeconds(wait2);

        flowAnomalyStartTime = Now();

        yield return StartCoroutine(DriftValue(
            () => system.flow,
            v => system.flow = v,
            flowAnomalyValue,
            driftDuration
        ));

        if (system != null)
            system.lockFlow = true;       // same idea for flow
    }

    IEnumerator DriftValue(System.Func<float> getter, System.Action<float> setter, float target, float duration)
    {
        float start = getter();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float v = Mathf.Lerp(start, target, t);
            setter(v);
            yield return null;
        }

        setter(target);
    }
}
