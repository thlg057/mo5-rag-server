
# Convert PNG to MO5 sprite (Thomson MO5)

Convert images into sprite data.

## Constraints

- 16 colors max
- 2 colors per 8 pixels
- width multiple of 8
- black background

## Command

```bash
python3 tools/scripts/png2mo5.py image.png --transparent
```

## Output

- form array
- color array

## Process

- analyze 8-pixel blocks
- encode bits and colors

## Common mistakes

- ❌ wrong palette
- ❌ width not aligned

Source: guide-graphical-development-mo5.md
