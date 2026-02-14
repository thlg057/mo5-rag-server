# Test the Thomson MO target macro THOMMO in CMOC

## Goal
Conditionally compile code when building for the Thomson MO target.

## Procedure
1. Compile with the Thomson MO target enabled:
   - `cmoc --thommo prog.c`
2. Wrap Thomson-specific code with the `THOMMO` preprocessor identifier:
   - Example:
     ```c
     #ifdef THOMMO
     /* Thomson MO target */
     #endif
     ```

## Notes
- The CMOC manual states that `--thommo` defines `THOMMO`.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Specifying the target platform”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html
