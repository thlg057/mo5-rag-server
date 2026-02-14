# Allocate dynamic memory with sbrk() in CMOC

## Goal
Allocate a raw block of memory at runtime using `sbrk()`.

## Procedure
1. Include the header:
   - `#include <cmoc.h>`
2. Allocate:
   - `void *p = sbrk(nbytes);`
3. Check failure:
   - failure is reported as `(void *) -1`
4. (Optional) Query remaining allocatable space:
   - `size_t n = sbrkmax();`

## Notes (per the manual)
- `sbrk()` returns a `void *`.
- The manual documents memory-layout assumptions for the CoCo and Dragon targets (allocation between program end and top of stack). If your target/loader does not match those assumptions (e.g., when relocating writable globals with `--data`), avoid `sbrk()`.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Dynamic memory allocation with sbrk()”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

