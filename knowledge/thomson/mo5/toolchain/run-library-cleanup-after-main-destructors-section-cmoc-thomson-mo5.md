# Run library cleanup code after main() with an LWASM destructors section

## Goal
Execute library code after `main()` returns (e.g., to release resources).

## Procedure
1. In a `.asm` file, define a `destructors` section.
2. Import any C cleanup function you want to call (with the leading underscore).
3. Call it using `LBSR`.
4. Ensure every line in the `.asm` file is indented.
5. Do not end the section with `RTS`/`PULS PC` (the manual states an `RTS` is generated after the concatenated sections).

## Notes
- The manual states the `.asm` file may define only one of `constructors` or `destructors`; it does not have to define both.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “User library constructors and destructors”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

