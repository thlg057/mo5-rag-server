# Use printf() with CMOC

## Goal
Print formatted text using CMOC’s `printf()` implementation.

## Procedure
1. Include the header:
   - `#include <cmoc.h>`
2. Use `printf()` with supported placeholders.

## Supported placeholders (per the manual)
- `%u`, `%d`, `%x`, `%X`, `%p`, `%s`, `%c`, `%f`, `%%`
- Field width is supported (except for `%f`).
- Left justification is supported.
- Zero padding is supported for integers (e.g., `%04x`, `%012ld`).
- The `l` modifier is supported for 32-bit `long` (e.g., `%012ld`).

## Notes
- `%p` prints a `$` prefix before hex digits; `%x`/`%X` do not.
- `%p`, `%x`, `%X` print A–F as capital letters.
- The manual describes `printf()` (and `putchar()` / `putstr()`) as outputting characters one-by-one via a platform routine (documented as Color Basic’s PUTCHR vector at `$A002`).

## Source
CMOC manual (v0.1.97, 2025-10-24), section “printf()”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

