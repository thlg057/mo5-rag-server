# Prefix global symbols with an underscore in LWASM modules used by CMOC

## Goal
Match CMOC’s C-to-assembly symbol naming convention when writing assembly modules.

## Procedure
1. Prefix each function name and global variable name with `_` in assembly.
   - If the C name is `foo`, the assembly symbol must be `_foo`.

## Notes
- The CMOC manual explicitly states: function names and global variable names must start with an underscore.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Using assembly language modules”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

