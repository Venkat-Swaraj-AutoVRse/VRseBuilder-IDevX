#  QAVOCheatCode

> ✨ A Unity tool for dynamically changing VO pitch during VR QA using Oculus controller combos.

---

##  Features

- Adjust VO playback speed **on-the-fly** in VR.
- Simple Oculus Touch **combo inputs**:
  - `X + A` = Decrease pitch
  - `Y + B` = Increase pitch
- Combo **cooldown system** to prevent spam.
- Includes **haptic feedback** for tactile confirmation.
- Pitch is clamped between min and max to maintain audio quality.

---

## Controller Combos

| Combo        | Action         | Result              |
|--------------|----------------|---------------------|
| `X` + `A`     | Decrease Pitch | Slows VO playback   |
| `Y` + `B`     | Increase Pitch | Speeds up VO        |

⚠️ Cooldown between activations is controlled by `_comboCooldownDuration`.

---

## Setup Instructions

1. **Add Component**: Attach `QAVOCheatCode.cs` to VO Player GameObject which will be under Default Systems

2. **Customize Settings** in the Unity Inspector:
   - `Pitch Increase Amount`
   - `Min Pitch`
   - `Max Pitch`
   - `Combo Cooldown Duration`

3. **Enter Play Mode** in VR (Oculus headset required).

---

## 🛠️ Inspector Settings

| Field                   | Description                                     |
|------------------------|-------------------------------------------------|
| `Pitch Increase Amount` | Amount to change pitch per combo (e.g. `0.3`)   |
| `Min Pitch`            | Minimum allowable pitch value (e.g. `0.1`)     |
| `Max Pitch`            | Maximum allowable pitch value (e.g. `3.0`)     |
| `Combo Cooldown`       | Time (in seconds) before another combo is valid |

---

## 📦 Requirements

- Unity **2020 or newer**
- **Oculus Integration SDK**

---

## ✅ Use Case

Need to slow down or speed up VO lines for testing. Just jump into VR, use the button combos, and test different playback speeds instantly.

## Future Plans
- To add more utility scripts to help speed up testing for QA Team and as well as developers
---
