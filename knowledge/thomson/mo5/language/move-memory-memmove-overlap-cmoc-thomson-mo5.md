# Move memory with memmove() when regions overlap (CMOC, Thomson MO5)

Move a block of memory safely when the source and destination regions may overlap.

## Steps

1. Include `<cmoc.h>`.
2. Call `memmove(dest, src, nBytes)`.

Source: `knowledge/docs/cmoc_h_mo5.md`
