# Create a static library archive with lwar for CMOC

## Goal
Group multiple `.o` object files into a static library (`lib*.a`) that CMOC can link with `-l`.

## Procedure
1. Create the archive with LWTOOLS `lwar`:
   - `lwar -c libstuff.a foo.o bar.o baz.o`
2. Name the archive with a `lib` prefix and a `.a` extension so it can be referenced as `-lstuff`.

## Notes
- The CMOC manual notes that if you pass `libstuff.a` directly on the CMOC command line, **all** object files are copied into the executable; using `-lstuff` allows the linker to pull only what is needed.

## Source
CMOC manual (v0.1.97, 2025-10-24), section “Creating libraries”: http://gvlsywt.cluster051.hosting.ovh.net/dev/cmoc-manual.html

