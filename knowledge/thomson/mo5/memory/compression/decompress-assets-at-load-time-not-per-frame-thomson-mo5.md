# Decompress assets at load time, not per frame (Thomson MO5)

Avoid decoding during gameplay.

## Rule
- decompress once at load
- never inside render loop

## Example
```c
rle_decode_ex(data, buffer);
```

Source: compression-rle-MO5.md
