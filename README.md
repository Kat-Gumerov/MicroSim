# MicroSim – Control Panel Micro-Anomaly Training Simulator

MicroSim is a Unity-based training simulation designed to help operators recognize and respond to micro-anomalies: small, gradual deviations that may indicate larger developing problems, without relying on alarms or step-by-step procedures.

The simulation places the user in a 3D environment with an interactive control panel. System indicators slowly drift outside of their normal range, and later the panel display freezes entirely. The user must notice abnormal behavior, record system status at regular intervals, and use conservative actions such as AUTO mode to stabilize the system when indications become unreliable.

---

## Training Objectives

MicroSim is designed to help operators practice how to:

- Detect subtle deviations before alarms occur  
- Monitor multiple interacting indicators simultaneously  
- Respond conservatively when system feedback becomes unreliable  
- Stabilize the system without scripted step-by-step instructions  
- Reflect on their response sequence via an after-action summary  

---

## Simulation Flow

### 1. Start and Instructions

- The user enters the control room in first-person view.
- Interacting with the control panel switches to a focused panel view.
- The scenario begins when the user presses **Start** on the instruction panel.
- A countdown timer runs to the next required **RECORD** event.

### 2. System Indicators and Controls

The system exposes three main indicators:

| Indicator   | Normal (Green) Range |
|------------|----------------------|
| Pressure   | 20–30                |
| Temperature| 25–35                |
| Flow       | 20–30                |

The user can influence the system via:

- **Pump Speed control**
- **Valve Position control**
- **AUTO / MANUAL mode toggle**

Indicators change dynamically based on these control inputs and internal relationships in the system model (e.g., pump speed increasing pressure and temperature, valve position affecting flow and relief).

### 3. Recording Events

The user is instructed to **record the system every 15 seconds**. Internally, the scenario tracks time from the moment the user presses Start.

- Every 15 seconds, the timer displays “Press RECORD.”
- When the user presses **RECORD**, the simulation logs:
  - Elapsed time since scenario start
  - The current values for each gauge
  - Whether each indicator is within its normal range
  - A summary tag: `OK` if all values are normal, `CHECK` if any are abnormal

Timestamps and drift times all use the same scenario-relative clock for consistency.

---

## Anomalies

### Anomaly 1 – Gradual Value Drift

After a configured delay from scenario start:

- Pressure gradually drifts toward a high value.
- Flow gradually drifts toward a low value.
- Temperature responds to these changes via the underlying system model.

This drift is gradual rather than step-like, encouraging the user to notice trends rather than obvious jumps. The user can use pump and valve controls to attempt to bring indicators back into their normal (green) bands.

### Anomaly 2 – Frozen Panel (Loss of Indication)

After the second recording is completed:

- The control panel indications visually freeze.
- Internal system values continue to evolve, but the user can no longer trust panel feedback.
- This condition persists until the user takes a conservative action.

The primary intended response is to switch from **MANUAL** to **AUTO** mode:

- AUTO mode moves the system back toward safe mid-range values.
- The freeze anomaly is cleared when AUTO is selected.
- The simulation records when the panel froze, when AUTO was enabled, and the resulting response time.

The system does not “self-heal” without operator input; anomalies persist until the operator acts.

---

## After-Action Summary

Once the third recording is completed:

1. The final line of the log is written (including “Scenario complete”).
2. The HUD remains visible briefly so the user can see the final state.
3. A summary panel appears with an after-action narrative.

The summary includes:

- A description of the status at each recording time:
  - Whether all indicators were in range or not
- When the pressure and flow drift anomalies began and their target values
- A description of temperature behavior:
  - Whether temperature rose with the pressure drift
  - Whether it was brought back into normal range by the end
- A description of the frozen-panel anomaly:
  - When indications froze
  - Whether AUTO mode was used
  - Approximate response time from freeze to AUTO activation

The emphasis is on describing what the operator did and how the system evolved, rather than grading performance with a numerical score.

---

## Controls

### First-Person Controls (Room View)

| Action                     | Input               |
|----------------------------|---------------------|
| Move                       | W / A / S / D       |
| Look                       | Mouse movement      |
| Interact with panel        | Click on panel      |
| Exit panel view            | Escape (ESC)        |

### Panel Controls (Panel View)

| Action                          | Input                                      |
|---------------------------------|--------------------------------------------|
| Record system state             | Click **RECORD** button                    |
| Start scenario                  | Click **START** button on instruction UI   |
| Toggle AUTO / MANUAL mode       | Click mode toggle switch/button            |
| Adjust pump speed up/down       | Click / Shift+Click on pump control        |
| Adjust valve position up/down   | Click / Shift+Click on valve control       |
| Exit summary and return to room | Click **EXIT** on summary panel            |

Notes:

- Simple **click** changes control values in one direction (e.g., up or down depending on control design).
- **Shift+Click** is used to adjust values in the opposite direction, allowing the user to move indicators both up and down via the same control.
- AUTO mode is a conservative recovery action: when enabled, the system gradually moves indicators back toward safe mid-range values while clearing the frozen panel anomaly.

---

## Technical Architecture (High-Level)

Key components:

- `SystemModel`  
  Encodes relationships between pump speed, valve position, pressure, temperature, and flow. Handles both manual and AUTO modes and enforces normal range clamping.

- `AnomalyController`  
  Schedules and executes gradual drift anomalies for pressure and flow based on scenario-relative time. Records anomaly start times and target values.

- `PanelFreezeAnomaly`  
  Freezes gauge, scope, and status light visuals when triggered. Tracks freeze start and clear times. Clears only when AUTO mode is selected and the system is reset to normal.

- `ScenarioManager` (ScenarioManagerB)  
  Manages scenario timing, record intervals, logging, and summary text generation. Tracks each recording’s time and whether indicators were in range.

- `PanelFocusManager`  
  Switches between room view (first-person FPS camera) and panel view (close-up panel camera). Disables player movement while in panel mode and restores it when returning to the room.

- `EnterPanelFocus`  
  Handles interaction with the panel object in the room, calling into `PanelFocusManager` to enter panel mode.

---

## Build Output

This project is designed to produce:

- A Windows executable prototype  
- Full Unity project source  
- Scenario configuration through easily adjustable fields (e.g., anomaly times, target values, recording interval)  
- In-scenario log output and a post-scenario summary for later review  

---

## Future Enhancements

Potential future work includes:

- Multiple difficulty levels with different anomaly timings and magnitudes  
- Randomized anomaly patterns to reduce predictability  
- Additional anomaly types (e.g., intermittent noise, conflicting indications)  
- Instructor or researcher dashboard for aggregating user runs and response metrics  
- Integration with VR or other immersive platforms  

---

## Project Context

This simulation was developed as part of a research-oriented training effort focused on understanding how operators detect and respond to emerging conditions in complex systems, particularly when guidance is limited and alarms may not yet be active.
