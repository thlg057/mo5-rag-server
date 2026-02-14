# Link a CMOC program with a library that ships a prepostmain.o object

## Goal
Link against a third-party library that requires a separate constructor/destructor object file (e.g., `prepostmain.o`).

## Procedure
1. Ensure the install directory contains both:
   - `prepostmain.o`
   - `libNAME.a`
2. Add `prepostmain.o` explicitly on the CMOC link command line.
3. Add the directory to the library search path with `-L` and reference the archive with `-l`.

## Notes (per the manual)
- The manual gives the pattern:
  - `/path/to/prepostmain.o -L /path/to -lNAME`
- `prepostmain.o` must be specified explicitly to force the linker to include all of its contents.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “User library constructors and destructors”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

