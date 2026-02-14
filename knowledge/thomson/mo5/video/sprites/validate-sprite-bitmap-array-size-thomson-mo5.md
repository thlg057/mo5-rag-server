# Validate sprite bitmap array size and layout (Thomson MO5)

Validate that a sprite bitmap array matches the expected MO5 1bpp layout before you use it in a draw routine.

## Steps

1. Pick the expected format:
   - 32x32: 32 rows x 4 bytes = 128 bytes
   - 16x16: 16 rows x 2 bytes = 32 bytes
2. Check that your array length matches the expected byte count.
3. Check that each row is stored contiguously (no gaps between rows).
4. Check that the most-significant bit of each byte corresponds to the leftmost pixel of that 8-pixel group.

Source: `knowledge/docs/mo5_sprite_designer_guide.md`, `knowledge/docs/mo5_graphics_corrected.md`
