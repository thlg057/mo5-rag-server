# Convert sprite row bits to hex bytes (Thomson MO5)

Convert a 1bpp sprite row (pixels) into the hex bytes you store in a C array.

## Steps

1. Write the row as bits (`0`=background, `1`=foreground), left to right.
2. Group bits by 8 (one byte).
3. Convert each group of 8 bits to hex.
4. Store bytes in row order (top to bottom).

## Example (16 pixels wide)

Row bits: `11110000 00001111`

- First byte `11110000` = `0xF0`
- Second byte `00001111` = `0x0F`

Store: `{ 0xF0, 0x0F }`

## Notes

- For 32 pixels wide, each row is 4 bytes.
- For 16 pixels wide, each row is 2 bytes.

Source: `knowledge/docs/mo5_sprite_designer_guide.md`
