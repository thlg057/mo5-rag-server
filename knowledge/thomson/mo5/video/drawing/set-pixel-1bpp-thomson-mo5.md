# Set pixel in 1bpp FORM bank (Thomson MO5)

Set or clear a single pixel by modifying one bit in the **FORM** VRAM bank.

## Steps

1. Check bounds: `x=0..319`, `y=0..199`.
2. Compute `offset = y*40 + x/8`.
3. Compute `mask = 1 << (7 - (x % 8))`.
4. Select the **FORM** bank.
5. OR the mask to set the pixel, AND with `~mask` to clear it.

## C example

```c
#define PRC  ((unsigned char *)0xA7C0)
#define VRAM ((unsigned char *)0x0000)

void set_pixel(int x, int y, unsigned char on)
{
    unsigned int offset;
    unsigned char mask;

    if ((unsigned)x >= 320u || (unsigned)y >= 200u) return;
    offset = (unsigned int)(y * 40u) + (unsigned int)(x >> 3);
    mask = (unsigned char)(1u << (7 - (x & 7)));

    *PRC |= 0x01; // FORM bank
    if (on) VRAM[offset] |= mask;
    else    VRAM[offset] &= (unsigned char)~mask;
}
```

Source: `knowledge/docs/mo5_graphics_corrected.md`
