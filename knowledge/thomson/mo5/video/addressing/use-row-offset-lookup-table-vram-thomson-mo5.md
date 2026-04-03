# Use row offset lookup table for VRAM (Thomson MO5)

Avoid computing y * width at runtime.

## Example
```c
offset = row_offsets[y] + x;
```

Source: mo5_optimization_guide.md
