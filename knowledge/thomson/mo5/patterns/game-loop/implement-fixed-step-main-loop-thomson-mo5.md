# Implement fixed-step main loop with frame counter (Thomson MO5)

Implement a simple game loop that updates logic once per frame and renders once per frame.

## Steps

1. Keep a frame counter.
2. Update input.
3. Update game state.
4. Render to VRAM.
5. Increment the frame counter and repeat.

## C skeleton

```c
unsigned int frame = 0;

for (;;) {
    // read input
    // update
    // render
    frame++;
}
```

## Notes

- If you need slower logic (e.g., 25 Hz), update only when `(frame & 1) == 0`.

Source: `knowledge/docs/mo5_game_dev_guide.md`
