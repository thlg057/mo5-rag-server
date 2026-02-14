# Initialize VRAM banks to black background and white foreground (Thomson MO5)

Initialize both VRAM banks so the screen has a deterministic attribute state and an empty bitmap.

## Steps

1. Select the **COLOR** bank (PRC bit 0 = 0).
2. Fill `$0000..$1F3F` with an attribute byte (e.g., black background / white foreground).
3. Select the **FORM** bank (PRC bit 0 = 1).
4. Fill `$0000..$1F3F` with `0x00` to show background everywhere.

## C example

```c
#define PRC  ((unsigned char *)0xA7C0)
#define VRAM ((unsigned char *)0x0000)

#define C_BLACK 0
#define C_WHITE 7
#define COLOR(bg, fg) (unsigned char)((bg & 0x0F) | ((fg & 0x0F) << 4))

unsigned int i;

*PRC &= (unsigned char)~0x01;          // COLOR bank
for (i = 0; i < 8000; i++) VRAM[i] = COLOR(C_BLACK, C_WHITE);

*PRC |= 0x01;                         // FORM bank
for (i = 0; i < 8000; i++) VRAM[i] = 0x00;
```

## Notes

- Writing COLOR first helps avoid “random” attributes when you later set FORM pixels.

Source: `knowledge/docs/mo5_video_doc.md`, `knowledge/docs/mo5_graphics_corrected.md`
