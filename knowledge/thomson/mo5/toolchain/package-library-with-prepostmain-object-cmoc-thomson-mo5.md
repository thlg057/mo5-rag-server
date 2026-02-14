# Package a CMOC library that uses constructors/destructors by shipping a prepostmain.o object

## Goal
Ship a third-party library that runs code before/after `main()` by providing a standalone constructor/destructor object (commonly named `prepostmain.o`).

## Procedure
1. Put constructor/destructor sections in a dedicated `.asm` file (e.g., `prepostmain.asm`).
2. Assemble it into an object file:
   - `lwasm -fobj -o prepostmain.o prepostmain.asm`
3. Do not include `prepostmain.o` inside the `.a` archive that contains the rest of the library.
4. Install `prepostmain.o` alongside the library archive (e.g., alongside `libtoolkit.a`).

## Notes (per the manual)
- The manual states `prepostmain.o` must stay outside the `.a` archive because otherwise the linker would not pull it in (no code refers to its contents explicitly).

## Source
CMOC manual (v0.1.97, 2025-10-24), section “User library constructors and destructors”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

