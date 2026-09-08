# RigMR

Open mixed-reality tooling for a **physical sim rig inside a VR cockpit**, targeting:

- Windows 11
- Official **PlayStation VR2 PC Adapter** (SteamVR / OpenXR)
- **Automobilista 2** first
- Predictive **steering-wheel mask** that leads the rendered wheel by ~30 ms

Not a finished SimMR clone. Predictor + host + profiles exist. Overlay does not.

## Honest status (2026-09-08)

| Piece | Status |
| --- | --- |
| Latency-compensating steering predictor | Implemented + unit tested |
| Fanatec DD1 Axis 1 → degrees (`FanatecAxis`) | Implemented + tested against App screenshots (540 / 900 / AUTO-1080 / 2520) |
| Wheel HID / DirectInput reader | Still stub — needs SharpDX on Windows |
| AMS2 `$pcars2$` reader | Implemented, untested on this install |
| Rig / wheel profiles | P5MP + DD1 name + 540° GT3 default |
| Host UDP `:24721` | Console host |
| PSVR2 overlay | Not in this repo |

## Quick start

```powershell
cd $env:USERPROFILE\RigMR
git pull
dotnet test
dotnet run --project src/RigMR.Host -- --synthetic --profile profiles/rig-p5mp.json --range-deg 540
```

Match `--range-deg` to the Fanatec Tuning Menu SEN. AUTO with no game on this DD1 is **1080**. Base in **PC Mode**. See `docs/fanatec.md`.

PNG masks belong in untracked `local/`. MIT. Not affiliated with Boosted Media, Sony, Reiza, Fanatec, or Valve.
