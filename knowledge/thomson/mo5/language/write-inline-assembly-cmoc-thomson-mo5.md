# Write inline assembly blocks with CMOC

## Goal
Embed 6809 assembly in C code using CMOC’s `asm { ... }` syntax.

## Procedure
1. Include CMOC headers as needed (e.g., `#include <cmoc.h>` for examples using `printf()`).
2. Write an inline assembly block:
   - `asm { /* 6809 instructions */ }`
3. Refer to C variables with a leading colon to avoid ambiguities:
   - example: `ldx :out`

## Label rules (per the manual)
- A label must start at the beginning of the line (no leading spaces/tabs).
- Labels must be unique program-wide, **or** use LWASM local-label conventions.
- One LWASM local-label convention is prefixing the label with `@` (local to the current block).

## Notes
- The manual recommends avoiding `$` as a local label marker in inline assembly, because it may hinder portability to OS-9.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Inline assembly”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

