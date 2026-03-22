
---

### `Source\Sensit.App.O2Characterizer\README.md`

```markdown
# Sensit.App.O2Characterizer

## Purpose

`Sensit.App.O2Characterizer` is a separate WinForms application used to collect and log O2 sensor characterization data.

This app is intended for **engineering characterization**, not final production pass/fail decisions.

It supports:
- sensor tracking
- live ADC sampling through the Raspberry Pi Pico bridge
- SQLite logging
- CSV export of run summaries and raw samples
- run tagging
- ambient metadata entry

## Branch

This README applies to branch:

- `feature/o2-characterization-app`

## Current Status

This app is functional for characterization work.

Confirmed working features:
- Create and store sensors
- Run live ADC characterization through Pico bridge
- Log run summaries and raw samples to SQLite
- Export runs to CSV
- Export raw samples to CSV
- Trend view of run averages
- Run tagging
- Ambient temperature / humidity entry

## Intended Use

Use this app to gather O2 characterization data before setting any production criteria.

At this stage, it should be used to study:
- repeatability
- short-term noise
- run-to-run drift
- effect of warm-up
- environmental influence

## Important Note

At this time, the app does **not** define a final production O2 pass/fail window.

A valid production acceptance window should only be created after collecting data from a meaningful sample size, such as:

- 30+ known-good boards/sensors

## Expected Test Condition

Current intended characterization condition:

- O2 board warmed for **5 minutes at 3.3 V**
- warm-up performed on the O2 burn-in fixture
- characterization run performed afterward in ambient air

## Live ADC Details

Current live ADC path uses the Raspberry Pi Pico I2C bridge.

### Current ADC settings
- I2C address: `0x48`
- Config bytes:
  - MSB: `0x44`
  - LSB: `0x83`

### Live run expectations
A successful live run should show:
- Run Mode = `Live ADC`
- COM port populated
- ADC address = `0x48`
- Config readback = `44 83`

## Database

The app uses a local SQLite database.

The database stores:
- sensors
- characterization runs
- raw characterization samples

If the database is deleted manually, the app will recreate it on next launch.

## Features

### Sensor management
- Add sensor by Sensor ID
- Store notes with sensor record

### Characterization runs
- Simulated mode
- Live ADC mode through Pico bridge
- Average, min, max, spread, and standard deviation calculation
- Raw sample logging with timestamps

### Run metadata
- Run tag
- Ambient temperature (manual entry)
- Ambient humidity (manual entry)

### CSV export
- Export run summaries
- Export raw samples for selected run

### Trend view
- Plot of average ADC count over time for the selected sensor

## Run Tags

Current run tag options:
- Known Good
- Engineering Sample
- Repeatability Test
- Warmup Study
- Suspect
- Retest

## Build

From repo root:

```powershell
dotnet build .\Source\Sensit.App.O2Characterizer\

## Scripts

Build the O2 Characterizer release build:

```powershell
.\Source\Sensit.App.O2Characterizer\Scripts\build_o2_characterizer.ps1