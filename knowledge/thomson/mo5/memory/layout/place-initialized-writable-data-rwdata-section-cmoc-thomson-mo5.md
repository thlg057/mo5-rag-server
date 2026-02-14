# Place initialized writable data in an LWASM rwdata section for CMOC

## Goal
Ensure writable globals that have static initializers are placed in the correct data section so they are loaded with initial contents.

## Procedure
1. Put writable globals with static initializers (typically arrays) in `SECTION rwdata` in your `.asm` module.

## Notes
- The CMOC manual states that **writable globals with static initializers must be in `rwdata`**.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

