# Enforce a maximum end address with CMOC --limit

## Goal
Make the build fail when the program would extend past an address limit.

## Procedure
- Pass `--limit=XXXX` (hex) when building/linking.

Example:
- `cmoc --thommo --limit=7800 prog.c`

## How it works (per the manual)
- CMOC compares the end of the program against the limit.
- The end address is indicated by the `program_end` symbol in the `.lst` listing.

## Notes
- The manual gives `--limit=7800` as an example to avoid ending too close to the system stack under Disk Basic.
- For Thomson targets, you still can use `--limit` as a coarse size guard, but pick a limit consistent with your actual loader/memory layout.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Enforcing a limit address on the end of the program”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

