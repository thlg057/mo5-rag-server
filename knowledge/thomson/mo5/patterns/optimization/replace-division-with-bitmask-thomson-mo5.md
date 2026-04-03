# Replace division with bitmask (Thomson MO5)

## Goal

Avoid slow division.

## Example

```c
value = rand() & 0x0F;
```

## Why

Bitwise ops are much faster than division.

## Pitfalls

- only works for power-of-two ranges
