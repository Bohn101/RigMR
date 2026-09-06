# Overlay milestone (not implemented)

Intended first implementation: a small OpenVR overlay process that listens for 48-byte pose packets on UDP 24721, loads a PNG of the physical rim, and rotates a world-locked quad by predictedDeg every vsync.

Do not inject into AMS2 first. Overlay-first is debuggable.

Optional camera layer: https://github.com/Obsidiate/psvr2passthrough
