
# Initialize graphics mode (Thomson MO5)

Enable bitmap mode and prepare VRAM.

## Goal

Start rendering in graphics mode.

## Example

```c
#include <mo5_video.h>

mo5_video_init(COLOR(C_BLACK, C_WHITE));
```

## Effects

- Enables graphics mode
- Clears form bank
- Fills color bank
- Precomputes row offsets

## When to call

- Once at startup

## Common mistakes

- ❌ Not calling → nothing displayed

Source: guide-graphical-development-mo5.md
