# Detect stack overflows at run time with CMOC --check-stack

## Goal
Catch stack overflows (e.g., runaway recursion, large local arrays) with runtime checks added by CMOC.

## Procedure
1. Build with `--check-stack`:
   - `cmoc --thommo --check-stack prog.c`
2. Install a stack overflow handler at program startup:
   - Call `set_stack_overflow_handler(handler)` early in `main()`.
3. Guard handler code with `_CMOC_CHECK_STACK_OVERFLOW_`:
   - CMOC defines `_CMOC_CHECK_STACK_OVERFLOW_` when `--check-stack` is used.

## Handler contract (per the manual)
- The handler receives:
  - the address of the failed check
  - the out-of-range stack pointer
- The handler must not return.

## Debug workflow (per the manual)
- Look up the address of the failed check in the generated `.lst` file to find where the overflow was detected.

## Notes
- The manual warns this option has a performance cost and is mainly recommended during debugging.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Detecting stack overflows at run time”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

