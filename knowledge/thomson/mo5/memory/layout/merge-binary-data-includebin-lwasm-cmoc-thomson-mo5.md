# Merge a binary file into a CMOC executable with LWASM INCLUDEBIN

## Goal
Embed an arbitrary binary file into your CMOC-linked executable and access it via start/end pointers.

## Procedure
1. Create an assembly file (e.g., `blob.asm`) that exports two symbols and includes the binary:
   - Put the data in `SECTION rodata` (or `SECTION rwdata` if you want it writable).
   - Export `_blob` and `_blob_end`.
   - Use `INCLUDEBIN blob.dat` between those labels.
2. Assemble to an object file:
   - `lwasm -fobj --output=blob.o blob.asm`
3. Link `blob.o` into your program by adding it to the CMOC link command.
4. In C, compute the embedded range by loading `_blob` and `_blob_end` addresses (the manual shows doing this via inline assembly with `leax _blob,pcr`).

## Notes (per the manual)
- There is no way to force the data to be loaded at a specific address; placement is decided by the linker and the `--org` origin.
- Prefix exported symbols with an underscore to match CMOC naming conventions and avoid conflicts.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Merging a binary file with the executable”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

