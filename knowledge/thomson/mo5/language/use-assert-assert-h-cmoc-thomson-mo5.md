# Use assert() by including <assert.h> in CMOC

## Goal
Abort a CMOC program when a runtime condition that must hold is false.

## Procedure
1. Include the header:
   - `#include <assert.h>`
2. Add assertions in your code:
   - `assert(expr);`

## Notes
- The CMOC manual describes `<assert.h>` as defining `assert()`, a macro that aborts the program if the assertion is false.
- For release builds, a common pattern is to define `NDEBUG` so assertions compile out (the manual mentions this as an alternative to heavier debugging options).

## Source
CMOC manual (v0.1.97, 2025-10-24), header list and debugging notes: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

