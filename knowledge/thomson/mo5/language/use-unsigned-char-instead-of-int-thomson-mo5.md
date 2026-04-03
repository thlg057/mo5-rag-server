# Use unsigned char instead of int (Thomson MO5)

Prefer 8-bit types over 16-bit for performance on 6809.

## Goal
Reduce instruction count and memory usage.

## Example
```c
for (unsigned char i = 0; i < 32; i++) {}
```

Source: mo5_optimization_guide.md
