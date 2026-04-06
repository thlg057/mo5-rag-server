
# Understand MO5 video memory layout (Thomson MO5)

Understand how pixels and colors are stored in VRAM.

## Goal

Correctly manipulate pixels and avoid hardware limitations.

## Video specs

- Resolution: 320x200
- 40 bytes per row

## Memory banks

1. Form bank:
   - 1 bit per pixel
   - Defines shape

2. Color bank:
   - 1 byte per 8 pixels
   - Format: FFFFBBBB

## Constraint

Each group of 8 pixels:
- max 2 colors
- foreground + background

## Coordinates

- X: 0–39 (bytes)
- Y: 0–199 (pixels)

## Why it matters

Violating constraints causes visual artifacts.

## Common mistakes

- ❌ Using more than 2 colors per block
- ❌ Thinking X is pixel-based

Source: guide-graphical-development-mo5.md
