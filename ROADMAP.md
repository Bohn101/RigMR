# Roadmap

## Milestone 0 — this commit

- [x] Predictor + tests that beat delayed-raw on a sine
- [x] 48-byte pose protocol
- [x] Host that broadcasts UDP
- [x] AMS2 SHM reader scaffold
- [x] Rig profile schema
- [x] PSVR2 / AMS2 docs

## Milestone 1 — usable on the sim PC

- [ ] SharpDX / raw HID wheel backend
- [ ] Confirm AMS2 field offsets against the copy of SharedMemory.h in the local install
- [ ] Measure real mask lag with a 240 fps phone, set `predictMs`
- [ ] OpenVR overlay quad + PNG

## Milestone 2 — mixed reality

- [ ] Sit-back + center-wheel calibration hotkey
- [ ] Extrude PNG to thickness, pivot height, vertical tilt
- [ ] Optional psvr2passthrough so hands stay visible
- [ ] Hide AMS2 virtual wheel

## Milestone 3 — only if milestone 2 feels good

- [ ] Dash / tub node editor (mouse + numpad, not AVP pinch)
- [ ] SimHub PNG import (images only)
- [ ] Second title (iRacing / ACC) behind the same `IWheelSource` + pose pipe
- [ ] Do **not** port CloudXR unless someone actually owns a Vision Pro

Will Ford’s own note still applies: making it work on one rig is not the same as supporting driver updates for a community.
