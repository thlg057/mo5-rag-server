# Generate a .bin executable in CoCo Disk Basic BIN format for Thomson targets

## Goal
Produce the CMOC output format used for Thomson targets, and account for the conversion gap.

## Procedure
1. Build for the Thomson MO target (example):
   - `cmoc --thommo -o prog.bin prog.c`
2. Treat the produced file as a CoCo Disk Basic BIN-format executable.

## Notes
- The CMOC manual states that for Thomson targets, the executable:
  - has the `.bin` extension, and
  - is in the CoCo Disk Basic BIN format.
- The manual also states CMOC does **not** provide tools to convert this BIN format to the Thomson computers’ native format.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Specifying the target platform”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html
