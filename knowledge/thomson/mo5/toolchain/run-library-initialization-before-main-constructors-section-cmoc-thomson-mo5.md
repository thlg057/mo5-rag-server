# Run library initialization code before main() with an LWASM constructors section

## Goal
Execute library code before `main()` (e.g., to initialize globals or resources).

## Procedure
1. Create a `.asm` file that defines a `constructors` section.
2. Import any C function you want to call from the constructor (with the leading underscore).
3. Call it using `LBSR` (as recommended by the manual to preserve relocatability).
4. Ensure every line in the `.asm` file is indented.
5. Do not end the section with `RTS`/`PULS PC` (the manual states CMOC generates a final return after concatenation).

## Source
CMOC manual (v0.1.97, 2025-10-24), section “User library constructors and destructors”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

