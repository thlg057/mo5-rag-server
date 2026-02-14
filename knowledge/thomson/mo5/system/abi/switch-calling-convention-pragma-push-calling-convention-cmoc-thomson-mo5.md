# Switch multiple function declarations to __gcccall with #pragma push_calling_convention

## Goal
Apply the GCC calling convention to a group of function declarations without repeating `__gcccall` on each prototype.

## Procedure
1. Add `#pragma push_calling_convention __gcccall` before the declarations.
2. Declare the affected functions.
3. Restore the default convention with `#pragma pop_calling_convention`.

## Example (from the manual)
- Functions declared between the pragmas use the GCC convention; others use the default CMOC convention.

## Notes
- The manual states these pragmas are available since CMOC 0.1.88.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “GCC calling convention”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

