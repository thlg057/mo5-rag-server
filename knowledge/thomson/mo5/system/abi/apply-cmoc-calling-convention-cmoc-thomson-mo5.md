# Apply the CMOC 6809 calling convention in hand-written code

## Goal
Write assembly and low-level code that interoperates correctly with CMOC-generated functions.

## Procedure
1. Pass parameters on the stack in reverse order (right-to-left).
2. Have the caller clean up the arguments after the call.
3. Respect integer promotions for `char` parameters:
   - `char` promotes to `int`
   - `unsigned char` promotes to `unsigned int`
4. Return values:
   - 8-bit return value in **B**
   - 16-bit return value in **D**
   - `struct`, `long`, `float`, `double`: return via a hidden first parameter (a pointer to the destination storage).
5. Preserve registers in callable routines:
   - CMOC-generated functions preserve **U**, **Y**, **S**, and **DP**.
   - Code is allowed to modify **A**, **B**, **X**, and **CC**.
6. Preserve **Y** in your own code for portability (the manual notes OS-9 uses Y for the data section, and the optimizer may use Y on other targets too).

## Notes
- The manual documents CMOC’s stack-frame setup using register **U** as a frame pointer when the function has parameters and/or locals.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Calling convention”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

