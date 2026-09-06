# Steering prediction

## Problem

The physical rim moves now. The mask you composite in the HMD is built from samples that are already old by the time photons hit the OLEDs.

Will Ford’s SimMR number on Vision Pro + CloudXR was ~35 ms of that pipe, so at 90 Hz he predicted ~2 frames. PSVR2 PC is a shorter pipe (no encode to a standalone headset), but a post-game overlay still needs a lead or the mask drags and then overshoots.

## Model

Let `theta` be unwrapped wheel angle in degrees.

Each sample `(t, theta)` updates velocity and acceleration with unwrap-aware EMAs, then:

    theta_hat(t+tau) = theta + omega*tau + 0.5*alpha*tau^2

Then clamp alpha, shrink tau on reversals so a flick does not overshoot, wrap back into the configured wheel range, and report confidence from jitter + time since last reversal.

Predict from HID, not from AMS2 steering. Shared memory is the sim's late view of the wheel.

Measure tau on your box with a 240 fps phone. Do not cargo-cult 30 ms.
