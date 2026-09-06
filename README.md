# RigMR

Open mixed-reality tooling for a **physical sim rig inside a VR cockpit**, targeting:

- Windows 11
- Official **PlayStation VR2 PC Adapter** (SteamVR / OpenXR)
- **Automobilista 2** first
- Predictive **steering-wheel mask** that leads the rendered wheel by ~30 ms so the physical rim and the virtual mask stay locked

This is **not** a finished clone of Boosted Media’s SimMR (Apple Vision Pro + CloudXR + custom visionOS compositor). That stack took ~100 hours of agent-assisted work *on the author’s own hardware*, and Vision Pro is a different device class. PSVR2-on-PC is a SteamVR HMD: the game already renders natively. The hard remaining problem is compositing a calibrated 3D mask of *your* wheel / dash / tub over passthrough and the sim cockpit, with the mask rotation *leading* HID latency.

## Honest status (2026-09-06)

| Piece | Status |
| --- | --- |
| Latency-compensating steering predictor | **Implemented + unit tested** |
| Wheel HID / DirectInput reader | Interface + synthetic source; SharpDX backend is Milestone 1 |
| AMS2 shared-memory telemetry reader | Implemented (Windows, `$pcars2$`) — verify offsets on your install |
| Rig / wheel / dash profile schema | Implemented |
| Host process (predict + broadcast pose) | Implemented (console + UDP `:24721`) |
| PSVR2 passthrough compositor | **Not in this repo** — use [Obsidiate/psvr2passthrough](https://github.com/Obsidiate/psvr2passthrough) as the camera layer |
| OpenXR / OpenVR overlay that draws the extruded wheel mesh | Scaffold + protocol only |
| In-headset node editor (pinch-drag like SimMR) | Not started (PSVR2 has no AVP hand mesh / eye pinch) |
| CloudXR / Vision Pro path | Out of scope |

You can run the predictor and host on the sim PC today. You cannot put on PSVR2 and see a finished 3D mask overlay from this commit alone.

## Why PSVR2 is a different problem than Vision Pro

SimMR on Vision Pro streamed the sim through NVIDIA CloudXR and ran a visionOS app. PSVR2 + official PC adapter is a wired SteamVR headset. AMS2 already speaks SteamVR / OpenXR. Do **not** add CloudXR — that would add latency.

Official PC runtime does not expose eye tracking. Passthrough is limited; use [psvr2passthrough](https://github.com/Obsidiate/psvr2passthrough) for the bottom cameras. Calibration is sit-back + centered wheel + a hotkey, not pinch-gaze.

## Quick start (Windows 11)

```powershell
dotnet test
dotnet run --project src/RigMR.Host -- --predict-ms 30 --range-deg 900 --synthetic
```

Pose packets go to UDP `127.0.0.1:24721`. See `docs/`.

MIT. Not affiliated with Boosted Media, Sony, Reiza, or Valve.
