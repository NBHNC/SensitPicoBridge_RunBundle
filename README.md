# Sensit Pico Bridge Programmer

This project replaces the Total Phase Aardvark with a Raspberry Pi Pico for smart sensor EEPROM programming.

The existing Windows C# programmer workflow is preserved. Barcode parsing, sensor type detection, EEPROM record generation, and CRC handling still happen in the original application. The Pico acts as a USB-to-I2C bridge and performs the low-level I2C transactions to the smart sensor through the fixture.

## What this does

- Removes dependency on the Aardvark
- Uses a Raspberry Pi Pico as the I2C interface
- Preserves the existing C# programming logic
- Supports programming through the O2 burn-in fixture board
- Allows use of standard USB barcode scanners through the Windows application

## Architecture

### PC Application
The Windows application:
- accepts barcode input
- parses the sensor barcode
- determines sensor type
- builds EEPROM records
- calculates CRC
- sends commands to the Pico over USB serial

### Raspberry Pi Pico
The Pico:
- appears as a COM port over USB
- receives bridge commands from the PC application
- performs I2C reads/writes to the smart sensor EEPROM

### Fixture Interface
The O2 burn-in board is used as the physical interface to the smart sensor board:
- `SDA`
- `SCL`
- `VCC`
- `GND`

Optional future additions:
- board-present `Sense`
- status LED output

## Repository Structure

```text
Docs/
Pico_Firmware/
Scripts/
Source/
SensitPicoBridge_RunBundle.sln
START_HERE.txt
```

### Key folders

- `Source/`  
  Contains the Windows application and SDK source

- `Pico_Firmware/`  
  Contains the MicroPython firmware for the Raspberry Pi Pico bridge

- `Scripts/`  
  Contains PowerShell build and run scripts

- `Docs/`  
  Contains setup, wiring, and bench test notes

## Hardware Required

- Raspberry Pi Pico or Pico W
- Smart sensor board
- O2 burn-in fixture board
- USB cable for Pico
- Windows PC
- USB barcode scanner configured as keyboard wedge
- Proper I2C pull-ups to 3.3 V if not already present on the bus

## Wiring

Current bridge wiring uses:

- `GP4 -> SDA`
- `GP5 -> SCL`
- `3V3 -> VCC`
- `GND -> GND`

The smart sensor runs on 3.3 V.

## Software Requirements

- Windows
- Visual Studio 2022 Community or Build Tools with MSBuild
- .NET SDK / targeting pack required by the solution
- Thonny or another MicroPython file uploader for Pico

## Important Build Note

This project must be built with **Visual Studio MSBuild**, not `dotnet build`.

The original SDK project contained COM-related build behavior that does not work through the `dotnet` CLI path. The build scripts in this repo use `vswhere` to locate `MSBuild.exe` and build the solution correctly.

## Setup

### 1. Load the Pico firmware

Copy these files to the Pico:

- `Pico_Firmware/main.py`
- `Pico_Firmware/pico_i2c_bridge.py`

The easiest way is with Thonny:
1. Connect the Pico
2. Open Thonny
3. Select MicroPython for Raspberry Pi Pico
4. Save both files to the device
5. Reboot the Pico

### 2. Verify the Pico is alive

Open a serial terminal to the Pico COM port and send:

```text
PING
```

Expected response:

```text
OK
```

### 3. Wire the Pico to the fixture

Connect:
- `GP4 -> SDA`
- `GP5 -> SCL`
- `3V3 -> VCC`
- `GND -> GND`

For the first test, leave optional `Sense` and `LED` lines disconnected unless you are actively validating them.

## Build

Open PowerShell in the repository root and run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Scripts\build_app.ps1
```

If the build succeeds, launch the app with:

```powershell
.\Scripts\run_app.ps1
```

## Manual Build with MSBuild

If needed, you can build manually:

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
& $msbuild ".\SensitPicoBridge_RunBundle.sln" /t:Build /p:Configuration=Release /p:Platform="Any CPU"
```

## Running the Programmer

1. Power the Pico over USB
2. Connect the Pico to the fixture
3. Insert a known-good smart sensor
4. Launch the Windows application
5. Open `File -> Configure Port`
6. Select the Pico COM port
7. Scan or type the barcode
8. Run the programming action

## Barcode Flow

The programmer:
1. reads the chemical sensor barcode
2. parses the sensor family from the barcode prefix
3. derives the required programming parameters
4. generates the EEPROM records
5. writes those records to the smart sensor EEPROM

The barcode identifies the sensor type and serial number. The application supplies the EEPROM parameter set.

## Supported Sensor Mapping

Current sensor type detection follows the existing application logic:

- `185` -> Oxygen
- `20`, `21`, `22` -> Hydrogen Sulfide
- `11`, `15` -> Carbon Monoxide
- `55` -> Hydrogen Cyanide
- `51` -> SO2 not yet supported

## EEPROM Records

The programmer writes:
- Base Record
- Device ID
- Manufacturing Record

These are written to the smart sensor EEPROM through the existing application logic.

## Scanner Support

The recommended scanner mode is:

- USB HID keyboard wedge
- Enter / carriage return suffix enabled

This allows most standard USB barcode scanners to work directly with the Windows application.

## Bench Test Checklist

1. Load Pico firmware
2. Verify `PING -> OK`
3. Wire Pico to fixture
4. Insert known-good sensor
5. Build and launch the app
6. Select Pico COM port
7. Program one known-good sensor by hand
8. Confirm successful write/readback behavior

## Troubleshooting

### PowerShell says script is not found
Make sure you are in the repository root before running:
```powershell
.\Scripts\build_app.ps1
```

### Build fails with COM reference errors
Use `MSBuild.exe` through the included script. Do not use `dotnet build`.

### Pico does not respond
- confirm firmware is loaded
- confirm correct COM port
- test with `PING`

### No I2C communication
- verify `SDA` and `SCL` are not swapped
- verify common ground
- verify 3.3 V supply
- verify pull-ups on the I2C bus
- verify the fixture is actually passing signals through

### Sensor does not program
- verify sensor is seated correctly
- verify EEPROM responds on the I2C bus
- test with a known-good sensor and barcode first

## Recent Fixes

- removed unused Office/VBIDE COM references from `Sensit.TestSDK.csproj`
- updated build script to use `vswhere` + Visual Studio `MSBuild.exe`
- updated run script to launch the built app correctly
- resolved build failures caused by COM reference resolution when using `dotnet build`

## Status

The project is now able to:
- build successfully on Windows
- run the existing programmer workflow
- communicate with the Pico bridge
- remove dependency on the Aardvark

## Next Improvements

Planned improvements may include:
- automatic read-back verification after programming
- `Sense`-based board-detect logic
- LED pass/fail indication
- streamlined operator workflow
- cleaner scanner-first UI behavior
