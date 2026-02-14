# Use putstr() for string output in CMOC

## Goal
Output a string using CMOC’s character-output path.

## Procedure
1. Call `putstr()` with a string to output.

## Notes (per the manual)
- The CMOC manual states `printf()`, like `putchar()` and `putstr()`, sends output one character at a time to Color Basic’s PUTCHR routine (vector at `$A002`).
- The manual also states the same redirection mechanism used for `printf()` output (via the `CHROUT` global variable) applies to `putstr()`.

## Source
CMOC manual (v0.1.97, 2025-10-24), output redirection notes: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

