# Set the writable data address with CMOC --data

## Goal
Place writable globals (the data section) at a chosen address.

## Procedure
- Pass `--data=XXXX` (hex) when linking.

Example (modular program):
1. `cmoc -c --thommo mod1.c`
2. `cmoc -c --thommo mod2.asm`
3. `cmoc --thommo -o prog.bin --org=C000 --data=3000 mod1.o mod2.o`

## Notes
- In modular builds, the manual states you should **not** use `#pragma data`; use `--data` at link time instead.
- Changing the data placement can invalidate assumptions made by some runtime helpers (e.g., allocation schemes) depending on target; verify with the target environment.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Specifying code and data addresses”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

