# Select VRAM bank using PRC register (Thomson MO5)

Switch between color and form VRAM banks.

## Procedure
- Clear bit 0 → color bank
- Set bit 0 → form bank

## Example
```c
*PRC &= ~0x01;
*PRC |=  0x01;
```

## Notes
Always select the correct bank before VRAM access.

Source: mo5_video.h
