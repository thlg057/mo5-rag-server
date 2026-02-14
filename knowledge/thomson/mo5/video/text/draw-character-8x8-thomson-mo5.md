# Draw 8x8 character bitmap into VRAM (Thomson MO5)

Render one 8×8 glyph by writing its attribute in the **COLOR** bank and its 1bpp rows in the **FORM** bank.

## Preconditions

- `tx` is the X position in bytes (0..39).
- `ty` is the Y position in text rows (0..24), where each row is 8 pixels tall.
- `font[8]` is an 8-byte glyph (one byte per scanline).

## Steps

1. For each of the 8 scanlines:
2. Compute `y = ty*8 + i`.
3. Compute `offset = y*40 + tx`.
4. Select COLOR bank and write the attribute for that byte cell.
5. Select FORM bank and write `font[i]`.

## C example

```c
#define PRC  ((unsigned char *)0xA7C0)
#define VRAM ((unsigned char *)0x0000)

void draw_char(int tx, int ty, const unsigned char font[8], unsigned char attr)
{
    unsigned int i;
    for (i = 0; i < 8; i++) {
        unsigned int y = (unsigned int)(ty * 8) + i;
        unsigned int offset = y * 40u + (unsigned int)tx;

        *PRC &= (unsigned char)~0x01; // COLOR
        VRAM[offset] = attr;

        *PRC |= 0x01; // FORM
        VRAM[offset] = font[i];
    }
}
```

Source: `knowledge/docs/mo5_video_doc.md`
