# Use color index constants 0 to 15 (Thomson MO5)

Define stable C constants for the MO5 fixed 16-color palette so you can build COLOR-bank attributes consistently.

## Steps

1. Define constants `0..15` for your project.
2. Use those indices with your `COLOR(bg, fg)` macro.

## C example

```c
#define C_BLACK 0
#define C_RED 1
#define C_GREEN 2
#define C_YELLOW 3
#define C_BLUE 4
#define C_MAGENTA 5
#define C_CYAN 6
#define C_WHITE 7
#define C_GRAY 8
#define C_LIGHT_RED 9
#define C_LIGHT_GREEN 10
#define C_LIGHT_YELLOW 11
#define C_LIGHT_BLUE 12
#define C_PURPLE 13
#define C_LIGHT_CYAN 14
#define C_ORANGE 15
```

Source: `knowledge/docs/mo5_video_doc.md`, `knowledge/docs/mo5_graphics_corrected.md`
