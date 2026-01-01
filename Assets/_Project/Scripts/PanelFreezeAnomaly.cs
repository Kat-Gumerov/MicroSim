using UnityEngine;

public class PanelFreezeAnomaly : MonoBehaviour
{
    [Header("Panel Elements")]
    public AnalogGauge[] gauges;
    public ScopeAnimator scopeAnimator;
    public StatusLight[] statusLights;

    [Header("System")]
    public SystemModel system;

    // state
    bool active;
    public bool Active => active;

    // timing info for summary
    public float freezeStartTime { get; private set; } = -1f;
    public float freezeClearTime { get; private set; } = -1f;

    public void ResetTracking()
    {
        freezeStartTime = -1f;
        freezeClearTime = -1f;
    }

    public void Trigger()
    {
        if (active) return;
        active = true;

        freezeStartTime = Time.timeSinceLevelLoad;
        freezeClearTime = -1f;

        // freeze gauges
        foreach (var g in gauges)
        {
            if (g != null)
                g.SetFrozen(true);
        }

        // freeze scope
        if (scopeAnimator != null)
            scopeAnimator.SetFrozen(true);

        // freeze status lights
        foreach (var l in statusLights)
        {
            if (l != null)
                l.SetFrozen(true);
        }

        Debug.Log("Panel Freeze Anomaly Triggered");
    }

    public void Clear()
    {
        if (!active) return;
        active = false;

        // unfreeze gauges
        foreach (var g in gauges)
        {
            if (g != null)
                g.SetFrozen(false);
        }

        // unfreeze scope
        if (scopeAnimator != null)
            scopeAnimator.SetFrozen(false);

        // unfreeze lights
        foreach (var l in statusLights)
        {
            if (l != null)
                l.SetFrozen(false);
        }

        Debug.Log("Panel Freeze Anomaly Cleared");
    }

    // called by ModeToggle whenever mode changes
    public void OnModeChanged(bool manualMode)
    {
        // only react if:
        //  1) the freeze anomaly is active
        //  2) operator just selected AUTO (manualMode == false)
        if (!active) return;
        if (manualMode) return;
        if (system == null) return;

        freezeClearTime = Time.timeSinceLevelLoad;

        // put system back into normal (green) range
        system.ResetToNormal();

        // unfreeze visuals
        Clear();

        Debug.Log("Panel Freeze cleared after AUTO mode selected via callback.");
    }
}
