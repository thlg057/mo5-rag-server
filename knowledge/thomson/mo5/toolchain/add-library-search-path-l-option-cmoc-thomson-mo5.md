# Add a library search path with CMOC -L

## Goal
Let CMOC/LWLINK search a custom directory for libraries referenced via `-l`.

## Procedure
1. Add a search directory with `-L`:
   - `cmoc --thommo -o prog.bin prog.c -L /path/to/libs -lNAME`

## Notes (per the manual)
- `-L` specifies a directory in which to search for libraries specified by `-l`.
- `-L` can be placed before or after the source/object files.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Creating libraries”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

