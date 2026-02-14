# Place read-only data in an LWASM rodata section for CMOC

## Goal
Ensure constants (including string literals) are linked into CMOC’s read-only data area.

## Procedure
1. In your `.asm` module, emit constants in `SECTION rodata`.
2. For string literals:
   - end the bytes with a null terminator (`0`)
   - encode `\n` as byte 10 (`$0A`) in the emitted bytes

## Notes
- The CMOC manual states that **read-only global variables and values must be in `rodata`**, and that this includes string literals.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

