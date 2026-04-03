# Use frame-locked game loop with VBL (Thomson MO5)

Synchronize entire loop with VBL.

## Example
```c
while (1) {
    mo5_wait_vbl();
    update();
    draw();
}
```

## Notes
Simple but mixes logic and rendering timing.

Source: vbl_mo5.md
