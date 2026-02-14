# Use divmod16() to compute quotient and remainder (CMOC, Thomson MO5)

Compute both the quotient and remainder of a 16-bit division in one call.

## Steps

1. Include `<cmoc.h>`.
2. Call `divmod16(dividend, divisor, &q, &r)`.

## C example

```c
#include <cmoc.h>

unsigned int q, r;
divmod16(1234, 10, &q, &r);
// q=123, r=4
```

Source: `knowledge/docs/cmoc_h_mo5.md`
