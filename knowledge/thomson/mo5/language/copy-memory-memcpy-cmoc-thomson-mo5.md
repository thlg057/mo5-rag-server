# Copy memory with memcpy() (CMOC, Thomson MO5)

Copy a contiguous block of memory from `src` to `dest`.

## Steps

1. Include `<cmoc.h>`.
2. Call `memcpy(dest, src, nBytes)`.

## Notes

- Use `memmove()` if the source and destination regions can overlap.

Source: `knowledge/docs/cmoc_h_mo5.md`
