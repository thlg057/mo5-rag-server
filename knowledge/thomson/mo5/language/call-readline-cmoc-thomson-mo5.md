# Call readline() with CMOC

## Goal
Read an input line into a NUL-terminated string using CMOC’s `readline()`.

## Procedure
1. Include the header:
   - `#include <cmoc.h>`
2. Call `readline()` and use the returned pointer.

## Notes (per the manual)
- `readline()` behaves like Basic’s `LINE INPUT`.
- It returns the address of a **global** buffer containing the entered text.
- The buffer is overwritten by the next `readline()` call.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “readline()”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

