# Build 16x16 1bpp sprite bitmap array (Thomson MO5)

Create a C byte array for a 16x16 sprite in MO5 1bpp format.

## Steps

1. Use 16 rows.
2. Encode each row as 16 bits = 2 bytes.
3. Concatenate rows from top to bottom.
4. Total size must be `16 * 2 = 32` bytes.

## C template

```c
// 16 rows, 2 bytes per row
const unsigned char sprite16[32] = {
    /* row 0 */ 0x00,0x00,
    /* row 1 */ 0x00,0x00,
    // ...
    /* row15 */ 0x00,0x00
};
```

Source: `knowledge/docs/mo5_sprite_designer_guide.md`
