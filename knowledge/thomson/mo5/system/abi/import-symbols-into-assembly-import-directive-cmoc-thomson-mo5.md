# Import symbols into an LWASM module with IMPORT for CMOC linking

## Goal
Reference a function or global variable that is defined in another module.

## Procedure
1. Add an `IMPORT` directive for each external symbol your assembly module expects.
2. Ensure imported symbols follow CMOC naming (prefix with `_`).

## Notes
- The CMOC manual states that functions/globals expected to be provided by other modules must be imported with an `IMPORT` directive.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

