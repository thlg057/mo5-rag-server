# Use position-independent code generation by default (CMOC, Thomson MO5)

Rely on CMOC's default position-independent code generation when you need your executable to be relocatable.

## Steps

1. Compile normally (CMOC generates position-independent code by default).
2. If you write inline assembly, keep it position-independent too.

## Notes

- Position-independent code makes it easier to load the same binary at different addresses.

Source: `knowledge/manuals/cmoc-manual.md`, `knowledge/docs/cmoc_thomson_mo5.md`
