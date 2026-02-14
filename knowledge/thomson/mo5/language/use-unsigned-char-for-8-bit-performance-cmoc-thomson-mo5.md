# Use unsigned char types for 8-bit performance (CMOC, Thomson MO5)

Use 8-bit types when possible to reduce code size and improve performance on the 6809.

## Steps

1. Prefer `unsigned char` for values in 0..255.
2. Promote to 16-bit (`unsigned int`) only when you need the extra range.

Source: `knowledge/docs/cmoc_h_mo5.md`
