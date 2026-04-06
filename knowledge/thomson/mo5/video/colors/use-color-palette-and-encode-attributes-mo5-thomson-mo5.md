
# Use color palette and encode attributes (Thomson MO5)

Define colors for rendering.

## Palette

16 fixed colors (0–15).

## Encoding

Each color byte:

FFFFBBBB

- F = foreground
- B = background

## Example

```c
COLOR(C_BLACK, C_WHITE)
```

## Usage

Used in:
- video init
- fill rect
- sprite colors

## Common mistakes

- ❌ Swapping fg/bg → inverted colors

Source: guide-graphical-development-mo5.md
