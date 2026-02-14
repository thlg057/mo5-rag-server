# Compute VRAM offset for 40x200 byte rows (Thomson MO5)

Compute the byte offset in the `$0000..$1F3F` VRAM window for a given pixel position on a 320×200 (40 bytes × 200 rows) layout.

## Steps

1. Convert pixel `x` to a byte index: `xByte = x / 8`.
2. Convert pixel `y` to a row offset: `rowBase = y * 40`.
3. Add them: `offset = rowBase + xByte`.
4. For 1bpp, compute bit position inside the byte: `bit = 7 - (x % 8)`.

## C example

```c
unsigned int offset = (unsigned int)(y * 40u) + (unsigned int)(x >> 3);
unsigned char mask = (unsigned char)(1u << (7 - (x & 7)));
```

## Notes

- Valid pixel range: `x = 0..319`, `y = 0..199`.
- `offset` is `0..7999` for each bank.

Source: `knowledge/docs/mo5_graphics_corrected.md`, `knowledge/docs/mo5_video_doc.md`
