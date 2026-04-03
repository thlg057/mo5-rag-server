# Generate .fd from binary (Thomson MO5)

## Concept

A disk image is composed of:
- tracks
- sectors (256 bytes)
- boot sector

## Algorithm

1. Create empty disk buffer
2. Insert boot sector
3. Copy binary into sectors
4. Respect sector layout

## Pitfalls

- wrong sector size
- incorrect ordering
