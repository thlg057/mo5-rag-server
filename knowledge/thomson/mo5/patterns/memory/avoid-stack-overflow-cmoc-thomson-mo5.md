# Avoid stack overflow in CMOC (Thomson MO5)

## Problem

CMOC stack is very limited.

## Solution

Use static/global variables.

## Example

```c
static unsigned char tmp;
```

## Pitfalls

- large local arrays → crash
