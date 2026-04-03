# Use late sync rendering with VBL (Thomson MO5)

Run logic first, then wait before drawing.

## Example
```c
while (1) {
    update();
    mo5_wait_vbl();
    draw();
}
```

## Notes
Maximizes CPU time for logic.

Source: vbl_mo5.md
