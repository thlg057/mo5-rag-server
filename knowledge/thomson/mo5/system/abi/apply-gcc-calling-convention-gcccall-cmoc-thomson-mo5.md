# Apply the GCC 6809 calling convention with __gcccall in CMOC

## Goal
Compile specific functions using CMOC’s GCC 6809 calling convention support.

## Procedure
1. Mark the function prototype or definition with `__gcccall`:
   - Example: `__gcccall int foo(char c, int w);`
2. If you use a prototype, ensure the call sites match the convention.

## Behavior (per the manual)
Compared to the default CMOC convention:
- The first 8-bit parameter is passed in **B** (instead of on the stack).
- The first 16-bit parameter is passed in **X** (instead of on the stack).
- A 16-bit return value is returned in **X** (instead of **D**).

## Notes
- If `__gcccall` is present on a function’s prototype but not on its definition, the manual states CMOC will compile the function using the GCC convention.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “GCC calling convention”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

