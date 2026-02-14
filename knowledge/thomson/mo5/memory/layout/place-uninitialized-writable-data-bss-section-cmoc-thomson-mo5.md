# Place uninitialized writable data in an LWASM bss section for CMOC

## Goal
Define writable globals without initializers (or initialized by code) without bloating the executable.

## Procedure
1. Put writable globals without static initializers in `SECTION bss`.
2. In the `bss` section, use only `RMB` directives.
3. Do not use `FCC`, `FDB`, or `FCB` in the `bss` section.

## Notes
- The CMOC manual states:
  - writable globals with no initializers (or initialized by running code in an `initgl` section) must be in `bss`
  - `bss` must contain only `RMB` so it takes no space in the executable file (at least with Disk Basic BIN format)

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

