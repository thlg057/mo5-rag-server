# Include the CMOC standard header <cmoc.h>

## Goal
Enable CMOC’s small standard library declarations (e.g., `printf()`, `sprintf()`, `readline()`, `sbrk()`).

## Procedure
1. Add this include in your C source file(s):
   - `#include <cmoc.h>`

## Notes
- The manual describes CMOC’s “standard library” as small and CMOC-specific in parts.
- Other headers exist (e.g., `<assert.h>`, `<setjmp.h>`, `<stdarg.h>`), but `<cmoc.h>` is the main entry point for CMOC-provided C functions.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “The standard library” / “Provided header files”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

