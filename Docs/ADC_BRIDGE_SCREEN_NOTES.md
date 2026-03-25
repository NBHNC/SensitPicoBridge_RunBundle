# ADC Bridge Screen Notes

## Purpose
This branch adds a basic ADS1115 bridge / solder sanity screen to the programmer flow.

Branch:
`feature/adc-bridge-screen`

## Scope
This is a **gross electrical screen** intended to catch obvious issues such as:

- ADC not responding on I2C
- bias/reference nodes reading incorrectly
- obvious rail-shorted ADC inputs
- possible QFN solder bridge symptoms

This is **not** a full analog characterization test.

## Current Behavior

### O2 boards
- O2 sensors must be **warmed up before programming**
- `AIN1` is treated as the known **1.2 V bias node**
- `AIN0` is the sensor output (`Vout`) and is warm-up dependent
- O2 `AIN0` behavior may vary when the sensor is cold

### Other supported sensor boards
- `AIN1` and `AIN3` are expected bias/reference nodes near **1.2 V**
- `AIN0` and `AIN2` are screened for obvious rail-short behavior

## Intent
The goal of this branch is to add a simple post-programming ADC sanity check without turning the programmer into a full sensor validation station.

## Notes
- Thresholds may need adjustment after additional bench testing
- This branch should remain separate from broader O2 characterization work
- Do not treat this feature as final analog validation

## Status
Initial implementation complete and pushed on branch:

`feature/adc-bridge-screen`
