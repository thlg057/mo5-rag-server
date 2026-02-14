# Draw 32x32 sprite in VRAM banks (Thomson MO5)

Draw a 32×32 1bpp sprite by writing a color attribute block in the **COLOR** bank and pixel bits in the **FORM** bank.

## Preconditions

- `tx` is the X position in **bytes** (0..36 for a 32px-wide sprite).
- `py` is the Y position in **pixels** (0..168 for a 32px-high sprite).
- `data` is 128 bytes (32 rows × 4 bytes).

## Steps

1. For each of the 32 rows:
2. Compute `offset = (py + row) * 40 + tx`.
3. Select COLOR bank and write the attribute byte to 4 consecutive bytes.
4. Select FORM bank and write the 4 bytes of sprite bits.

## C example

```c
#define PRC  ((unsigned char *)0xA7C0)
#define VRAM ((unsigned char *)0x0000)

void draw_sprite32(int tx, int py, const unsigned char *data, unsigned char attr)
{
    unsigned int row;
    for (row = 0; row < 32; row++) {
        unsigned int offset = (unsigned int)((py + (int)row) * 40) + (unsigned int)tx;

        *PRC &= (unsigned char)~0x01; // COLOR
        VRAM[offset]   = attr;
        VRAM[offset+1] = attr;
        VRAM[offset+2] = attr;
        VRAM[offset+3] = attr;

        *PRC |= 0x01; // FORM
        VRAM[offset]   = data[row*4u + 0u];
        VRAM[offset+1] = data[row*4u + 1u];
        VRAM[offset+2] = data[row*4u + 2u];
        VRAM[offset+3] = data[row*4u + 3u];
    }
}
```

Source: `knowledge/docs/mo5_graphics_corrected.md`, `knowledge/docs/mo5_game_dev_guide.md`
