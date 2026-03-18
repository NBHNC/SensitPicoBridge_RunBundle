Sensit Pico Bridge Run Bundle
=============================

What this bundle is
-------------------
This is the bridge-based replacement for the Aardvark. It keeps the existing
C# / WinForms Smart Sensor Programmer workflow and swaps only the transport
layer to a Raspberry Pi Pico over USB serial.

Included
--------
1. Source\Sensit.App.Programmer
   Full programmer source tree with the patched FormProgrammer.cs already in place.

2. Source\Sensit.TestSDK
   Full SDK source tree with PicoI2C.cs already added.

3. Pico_Firmware
   MicroPython bridge firmware for the Pico.

4. Scripts
   PowerShell scripts to build, run, and publish the Windows app.

5. Docs
   Setup notes, wiring notes, and scanner notes.

What changed
------------
- FormProgrammer.cs was replaced with the Pico bridge version.
- PicoI2C.cs was added to Sensit.TestSDK\Devices.
- The app .csproj was cleaned up so it does not ship aardvark.dll.
- A main.py wrapper was added for the Pico.

What did NOT change
-------------------
- SensorDataLibrary logic stays in C#.
- CRC logic stays in the existing C# SDK.
- The original WinForms operator flow stays in place.

Recommended use
---------------
Use the O2 burn-in board as the sensor interface board.
Use the Pico as the USB-to-I2C bridge.
Use the existing WinForms app on Windows.
Use a USB scanner in keyboard-wedge mode so it types into the barcode box.

Start here
----------
1. Read Docs\WINDOWS_BUILD_AND_RUN.txt
2. Read Docs\WIRING_O2_BURNIN_BOARD.txt
3. Flash the Pico using Scripts\flash_pico_instructions.txt
4. Build and run the app with Scripts\build_app.ps1 and Scripts\run_app.ps1
