# Fanatec Podium DD1 (this install)

Measured 2026-09-07/08 on WHITE_PWR, Fanatec App + driver **457**, firmware wheel-base **3.1.0.7**, motor **3.0.0.2**, rim id **Steering Wheel : 8** (Podium BMW M4 GT3 on the base). Base must be **PC Mode**, not Comp Mode CSW V2.5.

Official Fanatec SDK is partner-only. We do not ship it. Degrees come from DirectInput Axis 1 plus the current SEN value.

## Windows identity

| Field | Value |
| --- | --- |
| Settings name | `FANATEC Podium Wheel Base DD1` |
| DirectInput instance | `#2` seen in HID viewers |
| Steering | **Axis 1** (`-32767 … +32767`) |
| Product | `0x0EB7` / `037676` family |

The rim does not enumerate as its own game controller. The **base** is the HID device.

## Mapping (measured)

Axis 1 always rails at the *software* stop. The App degree readout is the physical angle.

```
theta_deg = (Axis1 / 32767) * (SEN / 2)
```

Treat `-32768` as `-32767`.

| SEN in Tuning Menu | App at left stop | Axis 1 at left stop | Lock to use |
| --- | --- | --- | --- |
| 540 | −270° | −32767 | 540 |
| 900 | −450° | −32767 | 900 |
| AUTO, no game | −540° | −32767 | **1080** |
| 2520 | −1260° (not captured at stop; −90° mid-travel confirmed) | ±32767 at stop | 2520 |

AUTO is not a number. With no game talking Fanatec Auto on this firmware the idle lock is **1080°**, not the 2520° FAQ default and not 1040.

`--range-deg` / `wheel.rangeDeg` must match the slider (or 1080 when Auto-idle). HID read of SEN is the follow-up so Auto can change mid-session without a flag.
