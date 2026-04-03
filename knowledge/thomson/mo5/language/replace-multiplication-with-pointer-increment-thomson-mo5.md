# Replace multiplication with pointer increment (Thomson MO5)

Use pointer arithmetic instead of index multiplication.

## Example
```c
unsigned char *src = data;
*dst = *src++;
```

Source: mo5_optimization_guide.md
