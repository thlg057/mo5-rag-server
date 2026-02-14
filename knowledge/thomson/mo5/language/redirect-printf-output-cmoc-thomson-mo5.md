# Redirect printf() output in CMOC

## Goal
Change where `printf()`/`sprintf()`/`putchar()`/`putstr()` send characters.

## Procedure
1. Implement a character output routine that receives the character in register A.
2. Install it with `setConsoleOutHook()`.
3. Optionally restore the previous hook later.

## Calling convention constraints (per the manual)
- The output routine receives the character in register **A**.
- It must preserve registers **B**, **X**, and **U**.
- It does not have to preserve **A**.

## Minimal installation pattern
- Declare a variable to store the previous hook:
  - `ConsoleOutHook oldHook;`
- Install:
  - `oldHook = setConsoleOutHook(newOutputRoutine);`
- Restore:
  - `setConsoleOutHook(oldHook);`

## Notes
- The manual states `printf()` writes via a routine whose address is stored in the library’s `CHROUT` global.
- Under the Color Basic environment, the manual notes that newline is character code **13** (not 10).

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Redirecting the output of printf()”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

