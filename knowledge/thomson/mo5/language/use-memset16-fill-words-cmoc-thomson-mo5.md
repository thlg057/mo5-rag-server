# Use memset16() to fill 16-bit words (CMOC, Thomson MO5)

Fill memory with a 16-bit value (word pattern) using CMOC's `memset16()` helper.

## Steps

1. Include `<cmoc.h>`.
2. Call `memset16(ptr, value, countWords)`.

## C example

```c
#include <cmoc.h>

// Fill 100 words (200 bytes) with 0x0000.
memset16((void *)0x4000, 0x0000, 100);
```

## Notes

- `countWords` is a number of 16-bit words, not bytes.

Source: `knowledge/docs/cmoc_h_mo5.md`
