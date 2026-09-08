# This rig

Same model as SimMR: one **rig** owns the cockpit mask + motion frame. **Wheels** and **dashes** are swappable profiles. We are not cloning Boosted Media UI or CloudXR.

## Platform — ProSimu P5MP

4-DoF-class plate: 4× PRS200 corners + PRS50 traction-loss. Up to 200 mm stroke, 280 mm/s, ~2 G, ~154×110×90 cm (225 cm with extensions), ~200 kg platform, ~350 kg payload.

A static cockpit mask is wrong the moment the plate moves. Seat-back calibration is only the *rest* pose. Live pose comes from the WT901C on the tub.

Still needed from you (tape + photo is enough):

- PRS200 stroke actually fitted (100 / 150 / 200)
- corner actuator spacing (front track, rear track, wheelbase of the four feet)
- PRS50 location vs rear axle line
- seat rails to wheel face (X) and to floor of tub (Z)
- IMU mount photo with axis labels

## IMU — Witmotion WT901C RS232 + CH340

9-axis (accel / gyro / mag + fused Euler / quaternion). Default 9600 10 Hz; for this job set **115200 / 200 Hz**. Protocol is WitMotion binary (`0x55` packets).

Do not use AMS2 telemetry as the *cockpit* pose. That is the virtual car. The IMU is the real room.

## Wheel base — Fanatec Podium DD1

See `docs/fanatec.md` for the measured Axis 1 / SEN table.

- Windows name: `FANATEC Podium Wheel Base DD1`
- Steering: DirectInput **Axis 1**, ±32767 = ±SEN/2
- Keep the base in **PC Mode**
- Default GT3 profile lock: **540°**. AUTO with no game on driver 457 = **1080°**
- Official SDK is NDA; we read HID + SEN, we do not bundle Fanatec binaries

## Wheel library (swap, do not bake into the rig)

1. Podium BMW M4 GT3 — `P_SW_BMW_GT3_H` (on the base now)
2. CSL Elite McLaren GT3 V2
3. ClubSport 918 RSR — owner PNG lives in untracked `local/`
4. ClubSport F1 2020
5. ClubSport BMW GT2
6. Leoxz XF1 Pro-3K

Shoot each wheel square-on, PNG with alpha. Do not commit vendor or SimHub art.
