# RigMR pose protocol

Little-endian. One datagram per update. 48 bytes.

```
offset  size  field
0       4     magic          R G M R
4       2     version        1
6       2     flags          bit0 = valid wheel, bit1 = ams2 live, bit2 = predicted
8       8     t_unix_us      host clock, microseconds
16      4     raw_deg        HID angle, centered 0
20      4     pred_deg       predicted angle used for the mask
24      4     omega_dps      deg/s
28      4     alpha_dps2     deg/s^2
32      4     confidence     0..1
36      4     predict_ms
40      4     ams2_steer     -1..1 if live, else NaN
44      4     reserved
```

UDP default: 127.0.0.1:24721.
