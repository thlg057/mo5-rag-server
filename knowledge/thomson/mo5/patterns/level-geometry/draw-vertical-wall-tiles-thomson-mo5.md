# Draw vertical wall tiles as 8x8 blocks (Thomson MO5)

Draw a vertical wall by repeating an 8x8 tile down a column.

## Steps

1. Choose a tile glyph/bitmap (8 bytes).
2. For each tile position, draw the 8x8 tile at `(tx, ty+i)`.

Source: `knowledge/docs/mo5_game_dev_guide.md`
