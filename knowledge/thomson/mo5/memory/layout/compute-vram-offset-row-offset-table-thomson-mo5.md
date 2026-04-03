# Compute VRAM offset using row offset table (Thomson MO5)

Avoid multiplication when computing screen offsets.

## Example
```c
offset = row_offsets[y] + x;
```

## Notes
Major optimization for rendering loops.

Source: mo5_video.h
