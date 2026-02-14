# Use sprintf() with CMOC

## Goal
Format text into a memory buffer (instead of printing it).

## Procedure
1. Include the header:
   - `#include <cmoc.h>`
2. Call `sprintf(dest, fmt, ...)`.

## Example
- `sprintf(greeting, "Hello, %s.", name);`

## Safety notes (per the manual)
- `sprintf()` writes into the destination buffer passed as its first argument.
- The caller must provide a buffer large enough for the formatted text **and** the terminating `\0`.
- The manual mentions that Standard C’s `snprintf()` is safer, but CMOC does not provide it (and suggests it could be implemented using output-redirection techniques).

## Source
CMOC manual (v0.1.97, 2025-10-24), section “sprintf()”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

