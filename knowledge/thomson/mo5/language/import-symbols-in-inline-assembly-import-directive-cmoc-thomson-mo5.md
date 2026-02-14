# Import external symbols in CMOC inline assembly with the LWASM IMPORT directive

## Goal
Use a global symbol provided by another module from inside a CMOC inline assembly block.

## Procedure
1. Inside the `asm { ... }` block, add an `IMPORT` directive for the symbol you use.
2. Use the imported symbol normally in the assembly instructions.

## Notes
- The CMOC manual states that if an inline assembly block uses a global variable provided by another module, an `IMPORT` directive must be included in that inline assembly block.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Importing symbols used by inline assembly”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

