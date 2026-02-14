# Avoid relying on volatile semantics (CMOC, Thomson MO5)

Avoid relying on the C `volatile` keyword for correctness, because CMOC accepts `volatile` but ignores it (and emits a warning).

## Steps

1. Treat `volatile` as documentation only when compiling with CMOC.
2. If you access memory-mapped I/O, make sure your code structure actually performs the required reads/writes.

Source: `knowledge/manuals/cmoc-manual.md`, `knowledge/docs/cmoc_thomson_mo5.md`
