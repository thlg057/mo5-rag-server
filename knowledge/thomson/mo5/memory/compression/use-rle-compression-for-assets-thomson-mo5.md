# Use RLE compression for assets (Thomson MO5)

Compress repetitive data to save memory.

## Goal

Maximize asset storage in limited RAM.

## Concept

Replace repeated bytes with (count, value).

## When to use

- Backgrounds
- Tilemaps
- Repetitive graphics

## When NOT to use

- Complex/noisy images

## Trade-offs

- Saves memory
- Costs CPU to decode

## Best practice

- Decompress once at load
- Never during frame rendering

## Pitfalls

- Overuse → slower runtime
- Poor compression ratio on random data

## Related

- decompress-assets-at-load-time

Source: compression doc
