# PSVR2 PC adapter notes

## Official Sony path

1. Official Sony PlayStation VR2 PC Adapter.
2. DisplayPort 1.4 into a real DP port on the GPU (not USB-C DP alt mode).
3. USB 3.0 directly to the PC.
4. Steam: PlayStation VR2 app + SteamVR as the OpenXR runtime.
5. Windows 11.

AMS2 already speaks SteamVR / OpenXR. Do not insert CloudXR.

Sony's *official* PC runtime still omits several PS5 features: HDR, eye tracking, headset rumble, adaptive triggers. That official list is what the first RigMR writeup cited. It is incomplete as a picture of what you can actually run in 2026.

## Community path (what you actually use)

[PSVR2Toolkit](https://github.com/BnuuySolutions/PSVR2Toolkit) (Whatdahopper / BnuuySolutions) is an unofficial replacement layer over Sony's SteamVR driver. Current advertised features:

- Eye tracking with calibration (SteamVR + OpenXR; mbucchia's separate OpenXR Eye Trackers layer is archived — uninstall it)
- Camera feed / Room View passthrough (some camera features need a headset jailbreak; read their wiki before you touch that)
- Adaptive triggers, improved controller prediction / haptics
- Headset vibration (jailbreak-gated)
- Developer CAPI

License on their README: free, **non-commercial**, eye-tracking data must not be used commercially. Do not pay anyone for a "toolkit zip."

Install from *their* wiki, not from memory of a YouTube transcript. Driver swap lives under:

`Steam/steamapps/common/PlayStation VR2 App/SteamVR_Plug-In/bin/win64`

### Dynamic foveated rendering

Eye tracking alone does not foveate a game. You still need a consumer of the gaze vector:

- OpenVR titles (e.g. Half-Life: Alyx): [PimaxMagic4All](https://github.com/mbucchia/PimaxMagic4All/wiki) / DFR-UI. NVIDIA-only per the LunchAndVR notes.
- OpenXR quad-views titles (DCS, Pavlov, and a short list): [Quad-Views-Foveated](https://github.com/mbucchia/Quad-Views-Foveated). Works on more GPUs.
- Community compatibility sheet: https://docs.google.com/spreadsheets/d/16GNwXAVCjUF9vCW6ubiUPQT00hZ7hRT5K_sbO6P9nYc/htmlview

Alyx launch options commonly used with DFR:

```
-console -vconsole +vr_fidelity_level_auto 0 +vr_fidelity_level 3 +vr_msaa 0
```

### AMS2

AMS2 is not on the short quad-views list. Treat DFR as a *maybe* until someone measures it. RigMR does not require foveation to place a wheel mask. Gaze *can* later drive a look-at-rim calibration instead of a pinch, if Toolkit CAPI + SteamVR eye pose is live on your box.

## What this means for RigMR

- Do not tell people eye tracking is impossible on PSVR2-PC. Official Sony: off. Toolkit: on.
- Do not vendor Toolkit into this repo (license + driver-swap risk).
- Optional later: read gaze from Toolkit CAPI / SteamVR for sit-back calibration assist.
- Passthrough for seeing the real rim is still Toolkit Room View and/or [psvr2passthrough](https://github.com/Obsidiate/psvr2passthrough).
