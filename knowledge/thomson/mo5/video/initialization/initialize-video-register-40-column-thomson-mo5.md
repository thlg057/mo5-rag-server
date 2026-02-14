# Initialize VIDEO_REG for 40-column video mode (Thomson MO5)

Enable the commonly used 40-column video configuration by setting bit 0 on `VIDEO_REG` (`$A7E7`).

## Steps

1. Define a pointer to `VIDEO_REG`.
2. Set bit 0.

## C example

```c
#define VIDEO_REG ((unsigned char *)0xA7E7)

*VIDEO_REG |= 0x01;
```

## Notes

- Keep other bits intact (use `|=` rather than assignment).

Source: `knowledge/docs/mo5_video_doc.md`, `knowledge/docs/mo5_graphics_corrected.md`, `knowledge/docs/mo5_game_dev_guide.md`
