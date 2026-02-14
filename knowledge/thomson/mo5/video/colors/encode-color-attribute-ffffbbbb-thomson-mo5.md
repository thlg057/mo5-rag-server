# Encode color attribute as FFFFBBBB (Thomson MO5)

Build one attribute byte for the **COLOR** VRAM bank, where the high nibble is the **foreground (FORM)** color and the low nibble is the **background (FOND)** color.

## Steps

1. Keep both indices in the `0..15` range.
2. Put background in the low nibble.
3. Put foreground in the high nibble.
4. Combine: `attr = (bg & 0x0F) | ((fg & 0x0F) << 4)`.

## C macro

```c
#define COLOR(bg, fg) (unsigned char)((bg & 0x0F) | ((fg & 0x0F) << 4))
```

## Notes

- With this convention, a bit set to `1` in the **FORM** bank shows the *foreground* (high nibble), and a bit set to `0` shows the *background* (low nibble).
- If you encounter code that uses the opposite nibble mapping, swap `bg`/`fg` at composition time.

Source: `knowledge/docs/mo5_video_doc.md`, `knowledge/docs/mo5_graphics_corrected.md`
