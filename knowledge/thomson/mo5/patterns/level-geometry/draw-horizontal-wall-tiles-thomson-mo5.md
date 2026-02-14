# Draw horizontal wall tiles as 8x8 blocks (Thomson MO5)

Draw a horizontal wall by repeating an 8x8 tile across a row.

## Steps

1. Choose a tile glyph/bitmap (8 bytes).
2. For each tile position, draw the 8x8 tile at `(tx+i, ty)`.

## Notes

- Using a tile loop reduces VRAM writes compared to per-pixel drawing.

Source: `knowledge/docs/mo5_game_dev_guide.md`
