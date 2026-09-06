# PSVR2 PC adapter notes

Native path:

1. Official Sony PlayStation VR2 PC Adapter.
2. DisplayPort 1.4 into a real DP port on the GPU (not USB-C DP alt mode).
3. USB 3.0 directly to the PC.
4. Steam: PlayStation VR2 app + SteamVR.
5. SteamVR set as the OpenXR runtime.
6. Windows 11.

AMS2 already speaks SteamVR / OpenXR. Do not insert CloudXR.

Official PC runtime drops HDR, adaptive triggers, most haptics, and eye tracking.

Passthrough on PC is limited. Community layer:
https://github.com/Obsidiate/psvr2passthrough

It injects the bottom stereo cameras. Top cameras are not exposed on PC.

Calibration is sit-back + centered wheel + a hotkey, not AVP pinch.
