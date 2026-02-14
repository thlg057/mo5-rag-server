# Set border color with PIA Port A (Thomson MO5)

Set the MO5 screen border (“cadre”) color by updating bits PA1..PA4 of the system 6821 PIA Port A at `$A700`, while preserving other bits (keyboard scan, tape control, etc.).

## Steps

1. Read the current value at `$A700`.
2. Clear bits 1..4 using mask `0xE1` (`1110 0001b`).
3. Insert the new 4-bit color value (0..15) shifted left by 1.
4. Write the result back to `$A700`.

## C example

```c
void set_border_color(unsigned char color)
{
    unsigned char *portA = (unsigned char *)0xA700;
    unsigned char v = *portA;

    v &= 0xE1;                    // keep PA0 and PA5..PA7
    v |= (unsigned char)((color & 0x0F) << 1);
    *portA = v;
}
```

## Notes

- Preserve PA0: the system uses it for keyboard scanning.
- If you reconfigure the PIA, ensure PA1..PA4 are configured as outputs before writing.

Source: `knowledge/docs/mo5_cadre_pia.md`, `knowledge/docs/Guide de Programmation Technique.md`, `knowledge/docs/mo5_guide_synthese.md`
