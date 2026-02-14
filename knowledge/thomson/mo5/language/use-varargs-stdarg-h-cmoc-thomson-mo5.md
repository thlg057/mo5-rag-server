# Use variable arguments with <stdarg.h> in CMOC

## Goal
Write a function that accepts a variable number of arguments (varargs).

## Procedure
1. Include the header:
   - `#include <stdarg.h>`
2. In the varargs function:
   - declare a `va_list`
   - call `va_start(list, lastNamedParam)`
   - read arguments with `va_arg(list, type)`
   - call `va_end(list)`

## Notes
- The CMOC manual describes `<stdarg.h>` as declaring the macros `va_start()`, `va_arg()` and `va_end()`.

## Source
CMOC manual (v0.1.97, 2025-10-24), header list: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

