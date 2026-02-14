# Disable CMOC peephole optimizations

## Goal
Reduce/disable CMOC’s peephole optimizer when debugging suspected miscompilations.

## Procedure
- Disable peephole optimizations:
  - `cmoc -O0 --thommo foo.c`
- Disable only some peephole optimizations:
  - `cmoc -O1 --thommo foo.c`
- Use default (full) optimization level:
  - `cmoc -O2 --thommo foo.c`

## Notes
- The manual describes `-O2` as equivalent to the default optimization level.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Disabling some optimizations”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

