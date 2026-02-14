# Select VRAM bank with PRC (Thomson MO5)

Use the PRC register (`$A7C0`) to select which VRAM bank is visible to the CPU in the `$0000..$1F3F` window.

## Steps

1. Define a pointer to PRC.
2. Write bit 0 to select the bank:
   - `0` = **COLOR** bank (attributes)
   - `1` = **FORM** bank (bitmap)

## C example

```c
#define PRC ((unsigned char *)0xA7C0)

// Select COLOR bank (attributes)
*PRC &= (unsigned char)~0x01;

// Select FORM bank (bitmap)
*PRC |= 0x01;
```

## Notes

- The VRAM window is multiplexed: you always access `$0000..$1F3F`, but PRC decides which physical 8 KB bank you are reading/writing.

Source: `knowledge/docs/mo5_video_doc.md`, `knowledge/docs/mo5_graphics_corrected.md`
