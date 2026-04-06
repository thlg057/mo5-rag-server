
# Implement main loop with VBL sync (Thomson MO5)

Create a stable real-time loop.

## Goal

Run game logic and rendering at 50 Hz.

## Example

```c
while (1) {
    mo5_wait_vbl();
    update_logic();
    draw();
}
```

## How it works

- Synchronizes with screen refresh
- Prevents tearing

## Performance

~20,000 cycles per frame

## Common mistakes

- ❌ Too much work per frame
- ❌ No VBL sync

Source: guide-graphical-development-mo5.md
