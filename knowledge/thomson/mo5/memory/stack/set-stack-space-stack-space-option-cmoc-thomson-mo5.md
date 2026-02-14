# Set reserved stack space with CMOC --stack-space

## Goal
Reserve a specific amount of stack memory so CMOC can size stack checking and `sbrk()` limits consistently.

## Procedure
1. Compile the C file that defines `main()` with `--stack-space=N` (decimal, N > 0):
   - `cmoc --thommo --stack-space=2048 prog.c`

## Notes (per the manual)
- This option affects:
  - the memory available to `sbrk()`
  - the checks inserted by `--check-stack`
- It must be specified when compiling the file that defines `main()`.
- It has no effect when compiling a file that does not define `main()`, and no effect when CMOC is invoked only to link object files.
- The manual notes this feature is not usable under OS-9, and not permitted when targeting the Vectrex.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Specifying the space allocated to the system stack”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

