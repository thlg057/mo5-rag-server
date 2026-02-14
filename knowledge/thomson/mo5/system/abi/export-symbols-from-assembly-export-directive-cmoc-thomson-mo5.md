# Export symbols from an LWASM module with EXPORT for CMOC linking

## Goal
Expose functions or global variables defined in an assembly module so they can be referenced by other CMOC/assembly modules.

## Procedure
1. Add an `EXPORT` directive for each symbol that must be visible to other modules.
2. Ensure exported symbols follow CMOC naming (prefix with `_`).

## Notes
- The CMOC manual states that functions and globals that must be available to other modules must be exported with an `EXPORT` directive.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

