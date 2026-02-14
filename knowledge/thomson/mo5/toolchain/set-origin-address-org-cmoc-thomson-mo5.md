# Set the program origin address with CMOC --org

## Goal
Choose the executable’s load/start address at link time.

## Procedure
- Pass `--org=XXXX` (hex) when linking:
  - Single-step build: `cmoc --thommo --org=2800 prog.c`
  - Separate compilation + link:
    1) `cmoc -c --thommo mod1.c`
    2) `cmoc -c --thommo mod2.asm`
    3) `cmoc --thommo -o prog.bin --org=C000 mod1.o mod2.o`

## Notes
- In modular builds, the manual states you should **not** use `#pragma org`; use `--org` at link time instead.

## Source
CMOC manual (v0.1.97, 2025-10-24), sections “Origin address” and “Specifying code and data addresses”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

