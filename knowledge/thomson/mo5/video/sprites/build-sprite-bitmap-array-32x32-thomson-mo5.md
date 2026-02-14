# Build 32x32 1bpp sprite bitmap array (Thomson MO5)

Create a C byte array for a 32x32 sprite in MO5 1bpp format.

## Steps

1. Use 32 rows.
2. Encode each row as 32 bits = 4 bytes.
3. Concatenate rows from top to bottom.
4. Total size must be `32 * 4 = 128` bytes.

## C template

```c
// 32 rows, 4 bytes per row
const unsigned char sprite32[128] = {
    /* row 0 */ 0x00,0x00,0x00,0x00,
    /* row 1 */ 0x00,0x00,0x00,0x00,
    // ...
    /* row31 */ 0x00,0x00,0x00,0x00
};
```

Source: `knowledge/docs/mo5_sprite_designer_guide.md`
