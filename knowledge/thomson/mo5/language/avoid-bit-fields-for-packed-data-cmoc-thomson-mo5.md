# Avoid bit fields for packed data (CMOC, Thomson MO5)

Avoid using C bit fields when you need packed bit-level storage, because CMOC allocates bit fields using the declared base type regardless of the specified bit width.

## Steps

1. Use explicit bit masks on `unsigned char`/`unsigned int` instead of bit fields.
2. Pack/unpack bits manually when you need a strict layout.

Source: `knowledge/manuals/cmoc-manual.md`, `knowledge/docs/cmoc_thomson_mo5.md`
