# Compile a C program with CMOC for Thomson MO

## Goal
Build a `.bin` executable for the Thomson MO target.

## Prerequisites (must be in PATH)
- `cmoc`
- a C preprocessor callable as `cpp`
- LWTOOLS:
  - `lwasm` (assembler)
  - `lwlink` (linker)
  - (`lwar` only if you create libraries)

## Procedure
1. Compile/link a single-file program:
   - `cmoc --thommo foo.c`
2. See available options:
   - `cmoc --help`

## Notes
- The manual documents a default origin of `$2800` for “Thomson MO/TO”.
- For Thomson targets, the output is a `.bin` in CoCo Disk Basic BIN format (conversion to Thomson-native format is not provided by CMOC).

## Source
CMOC manual (v0.1.97, 2025-10-24), sections “Compiling a C program”, “Default code addresses”, “Specifying the target platform”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

