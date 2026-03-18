# pico_i2c_bridge.py
# Flash this file to your Raspberry Pi Pico as "main.py".
#
# The script listens on the USB-CDC serial port for single-line text commands
# and translates them into I2C transactions, responding with "OK [hex data]"
# or "ERROR message".
#
# Supported commands
# ------------------
#   PING
#       → OK
#
#   POWER ON
#       → OK        (drives GP22 high — enable EEPROM VCC via transistor)
#
#   POWER OFF
#       → OK        (drives GP22 low  — disable EEPROM VCC)
#
#   WRITE <addr_hex> <b0_hex> [b1_hex ...]
#       → OK        (I2C write to 7-bit address <addr_hex>)
#
#   READ <addr_hex> <n_dec>
#       → OK <b0_hex> <b1_hex> ...   (I2C read of <n_dec> bytes)
#
#   WRRD <addr_hex> <n_read_dec> <b0_hex> [b1_hex ...]
#       → OK <b0_hex> <b1_hex> ...   (I2C write then read with repeated START)
#
# Wiring
# ------
#   GP4  (pin 6)  — SDA  — connect to EEPROM SDA (+ 4.7 kΩ pull-up to 3.3 V)
#   GP5  (pin 7)  — SCL  — connect to EEPROM SCL (+ 4.7 kΩ pull-up to 3.3 V)
#   GP22 (pin 29) — PWR  — optional: base/gate of transistor for switched VCC
#   GND  (pin 38) — GND  — shared ground with EEPROM
#
# If you power the EEPROM directly from the Pico's 3.3 V rail (pin 36) and do
# not need switched power, leave GP22 unconnected.  The POWER commands will
# still return "OK" without any electrical effect.
#
# I2C frequency is set to 400 kHz (Fast mode) matching the original Aardvark
# configuration.

import sys
from machine import I2C, Pin

# ---------------------------------------------------------------------------
# Hardware configuration — adjust these pin numbers if needed.
# ---------------------------------------------------------------------------

I2C_ID  = 0      # Use hardware I2C0
SDA_PIN = 4      # GP4  — physical pin 6
SCL_PIN = 5      # GP5  — physical pin 7
PWR_PIN = 22     # GP22 — physical pin 29 (optional switched power output)
I2C_FREQ = 400_000  # 400 kHz Fast-mode

# ---------------------------------------------------------------------------
# Initialise peripherals
# ---------------------------------------------------------------------------

i2c = I2C(I2C_ID, sda=Pin(SDA_PIN), scl=Pin(SCL_PIN), freq=I2C_FREQ)
power_pin = Pin(PWR_PIN, Pin.OUT, value=0)  # default: power off

# ---------------------------------------------------------------------------
# Command handlers
# ---------------------------------------------------------------------------

def cmd_ping(_parts):
    return "OK"


def cmd_power(parts):
    if len(parts) < 2:
        return "ERROR POWER requires ON or OFF"
    arg = parts[1].upper()
    if arg == "ON":
        power_pin.value(1)
        return "OK"
    if arg == "OFF":
        power_pin.value(0)
        return "OK"
    return f"ERROR unknown POWER argument: {parts[1]}"


def cmd_write(parts):
    # WRITE <addr_hex> <b0_hex> [b1_hex ...]
    if len(parts) < 3:
        return "ERROR WRITE requires an address and at least one data byte"
    try:
        addr = int(parts[1], 16)
        data = bytes(int(b, 16) for b in parts[2:])
        i2c.writeto(addr, data)
        return "OK"
    except Exception as exc:
        return f"ERROR {exc}"


def cmd_read(parts):
    # READ <addr_hex> <n_dec>
    if len(parts) < 3:
        return "ERROR READ requires address and byte count"
    try:
        addr = int(parts[1], 16)
        n    = int(parts[2])
        data = i2c.readfrom(addr, n)
        return "OK " + " ".join(f"{b:02X}" for b in data)
    except Exception as exc:
        return f"ERROR {exc}"


def cmd_wrrd(parts):
    # WRRD <addr_hex> <n_read_dec> <b0_hex> [b1_hex ...]
    # Performs: WRITE (no STOP) → repeated START → READ
    if len(parts) < 4:
        return "ERROR WRRD requires address, read length, and write data"
    try:
        addr   = int(parts[1], 16)
        n_read = int(parts[2])
        wdata  = bytes(int(b, 16) for b in parts[3:])
        # stop=False keeps the bus busy so the next operation issues a
        # repeated START rather than a STOP + START.
        i2c.writeto(addr, wdata, False)
        rdata = i2c.readfrom(addr, n_read)
        return "OK " + " ".join(f"{b:02X}" for b in rdata)
    except Exception as exc:
        return f"ERROR {exc}"


DISPATCH = {
    "PING":  cmd_ping,
    "POWER": cmd_power,
    "WRITE": cmd_write,
    "READ":  cmd_read,
    "WRRD":  cmd_wrrd,
}


def handle_line(line: str) -> str:
    parts = line.strip().split()
    if not parts:
        return "ERROR empty command"
    verb = parts[0].upper()
    handler = DISPATCH.get(verb)
    if handler is None:
        return f"ERROR unknown command: {verb}"
    return handler(parts)


# ---------------------------------------------------------------------------
# Main loop — read lines from USB serial and dispatch commands
# ---------------------------------------------------------------------------

# Signal readiness to the host.
print("Pico I2C Bridge ready")
sys.stdout.flush()

while True:
    line = sys.stdin.readline()
    if line:
        response = handle_line(line)
        print(response)
        sys.stdout.flush()
