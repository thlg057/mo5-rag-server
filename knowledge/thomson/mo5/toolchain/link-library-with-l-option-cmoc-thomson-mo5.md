# Link a static library with CMOC -l

## Goal
Link a `libNAME.a` archive into your CMOC program while pulling only the required objects.

## Procedure
1. Build or install the library as `libNAME.a`.
2. Pass `-lNAME` when linking:
   - `cmoc --thommo -o prog.bin prog.c -lNAME`
3. Ensure `-lNAME` appears **after** the source/object files on the CMOC command line.

## Notes
- The CMOC manual recommends naming libraries `lib*.a` to be compatible with `-l`.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Creating libraries”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

