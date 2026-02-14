# Select the Thomson MO target with --thommo in CMOC

## Goal
Compile and link a CMOC program for the Thomson MO target.

## Procedure
1. Compile and link using the Thomson MO target switch:
   - `cmoc --thommo prog.c`

## Notes
- The CMOC manual states that `--thommo` makes the compiler define the preprocessor identifier `THOMMO`.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Specifying the target platform”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html
