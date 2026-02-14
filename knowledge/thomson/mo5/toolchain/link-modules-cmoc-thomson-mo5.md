# Link multiple modules with CMOC for Thomson MO

## Goal
Compile several `.c` files separately and link them into one Thomson MO executable.

## Procedure
1. Compile each module to an object file (`.o`) using the same target switch:
   - `cmoc -c --thommo mod1.c`
   - `cmoc -c --thommo mod2.c`
2. Link the object files into a final executable (also with the same target switch):
   - `cmoc --thommo -o prog.bin mod1.o mod2.o`

## Notes
- The manual requires every compilation unit **and** the final link step to use the same target command-line switch.
- If `lwasm` or `lwlink` are not in your PATH, CMOC allows specifying their paths via `--lwasm=` and `--lwlink=`.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Modular compilation and linking” / “Target specification”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

