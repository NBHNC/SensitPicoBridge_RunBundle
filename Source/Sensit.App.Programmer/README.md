# Sensit.App.Programmer

## Purpose

`Sensit.App.Programmer` is the WinForms application used to program and verify Smart Sensor boards through the Raspberry Pi Pico I2C bridge.

This branch adds a **simple ADC functional test** to the programmer so the application can confirm the sensor board is alive and responding after programming.

## Branch

This README applies to branch:

- `feature/adc-functional-test`

## What Changed

This branch adds a basic ADC validation step into the programmer workflow.

### Added behavior
- ADC check is called during both **Read** and **Write** flows
- ADS111x configuration is written and read back
- Conversion samples are read from the ADC
- Average ADC counts are shown in the UI in **Sensor Counts**
- The test fails if:
  - ADC config readback is wrong
  - conversion data is all zero
  - conversion data appears stuck at the rail

### Current intent
This is a **board functional / ADC alive** test only.

It is **not yet** a final sensor acceptance test with production criteria by gas type.

## Current ADC Configuration

Current settings used by the functional test:

- I2C address: `0x48`
- Config register pointer: `0x01`
- Conversion register pointer: `0x00`
- Config bytes:
  - MSB: `0x44`
  - LSB: `0x83`

## Current Pass Logic

The current ADC test checks for:

1. Successful config write/readback
2. Successful conversion reads
3. Conversion data is not all zero
4. Conversion data is not stuck at the rail

## Limitations

- No validated O2/CO/H2S/HCN count windows yet
- `sensorType` is not yet used for gas-specific acceptance criteria
- This should not be treated as a finalized production gas validation method

## Build

From repo root:

```powershell
dotnet build .\Source\Sensit.App.Programmer\