# Use setjmp() and longjmp() by including <setjmp.h> in CMOC

## Goal
Implement a non-local jump (nonlocal goto) for error handling or early exits across multiple stack frames.

## Procedure
1. Include the header:
   - `#include <setjmp.h>`
2. Save an execution context with `setjmp()`.
3. Jump back to that context with `longjmp()`.

## Notes
- The CMOC manual describes `<setjmp.h>` as declaring `setjmp()` and `longjmp()` and notes they can be used to perform a nonlocal goto.

## Source
CMOC manual (v0.1.97, 2025-10-24), header list: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

