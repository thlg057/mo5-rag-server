# Fill rectangle in VRAM with a solid color (Thomson MO5)

Fill a rectangular region by writing the same attribute in the **COLOR** bank and forcing all pixels to background (FORM = 0).

## Preconditions

- Coordinates are expressed in **bytes** for X (`xByte`, `widthBytes`) and in **pixel rows** for Y (`y`, `height`).

## Steps

1. Select COLOR bank and write `COLOR(color, color)` for each byte in the rectangle.
2. Select FORM bank and write `0x00` for each byte in the same region.

## C example

```c
#define PRC  ((unsigned char *)0xA7C0)
#define VRAM ((unsigned char *)0x0000)
#define COLOR(bg, fg) (unsigned char)((bg & 0x0F) | ((fg & 0x0F) << 4))

void fill_rect(int xByte, int y, int widthBytes, int height, unsigned char c)
{
    int i, j;
    *PRC &= (unsigned char)~0x01;
    for (j = 0; j < height; j++)
        for (i = 0; i < widthBytes; i++)
            VRAM[(y + j) * 40 + (xByte + i)] = COLOR(c, c);

    *PRC |= 0x01;
    for (j = 0; j < height; j++)
        for (i = 0; i < widthBytes; i++)
            VRAM[(y + j) * 40 + (xByte + i)] = 0x00;
}
```

Source: `knowledge/docs/mo5_video_doc.md`
