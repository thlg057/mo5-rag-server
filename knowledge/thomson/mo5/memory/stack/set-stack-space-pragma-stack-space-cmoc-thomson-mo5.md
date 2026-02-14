# Set reserved stack space with #pragma stack_space in CMOC

## Goal
Reserve a specific amount of stack memory using a pragma (instead of the command line).

## Procedure
1. In the C file that defines `main()`, set the pragma:
   - `#pragma stack_space N`
2. Compile normally (optionally still using `--thommo`).

## Notes (per the manual)
- The command-line option `--stack-space=N` takes precedence over this pragma.
- The pragma must be in the compilation unit that defines `main()` to have an effect.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Specifying the space allocated to the system stack”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

