# Handle ZSQD movement keys (Thomson MO5)

Map Z/S/Q/D keys to player movement in a consistent way.

## Steps

1. Read one key.
2. Normalize it (e.g., treat lowercase/uppercase the same).
3. Update position based on the key.

## C example

```c
switch (key) {
case 'Z': case 'z': y -= 1; break; // up
case 'S': case 's': y += 1; break; // down
case 'Q': case 'q': x -= 1; break; // left
case 'D': case 'd': x += 1; break; // right
default: break;
}
```

Source: `knowledge/docs/mo5_game_dev_guide.md`
