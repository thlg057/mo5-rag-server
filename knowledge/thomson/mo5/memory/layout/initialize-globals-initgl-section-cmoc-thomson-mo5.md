# Initialize globals from assembly by using an LWASM initgl section in CMOC

## Goal
Run assembly code at startup to initialize global variables.

## Procedure
1. Put global-initialization code in `SECTION initgl`.
2. Do not end the `initgl` section with an `RTS` instruction.

## Notes
- The CMOC manual states that the *exception* to placing code in `SECTION code` is code that initializes global variables, which must be in a section named `initgl`.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

