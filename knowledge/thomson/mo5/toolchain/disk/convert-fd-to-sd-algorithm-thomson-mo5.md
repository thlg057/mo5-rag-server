# Convert .fd to .sd algorithm (Thomson MO5)

## Concept

- read 256 bytes
- write sector
- write padding
- repeat

## Key rule

Missing sectors → fill with 0xFF

## Structure

- 4 faces × 80 tracks
- 16 sectors

## Result

Compatible with MO5/TO7
