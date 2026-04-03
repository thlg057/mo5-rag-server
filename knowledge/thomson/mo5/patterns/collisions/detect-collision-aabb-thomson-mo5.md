# Detect collision using AABB (Thomson MO5)

## Goal

Fast collision detection.

## Example

```c
(ax < bx + bw) && (ax + aw > bx)
```

## Performance

~20 cycles

## When to use

- real-time games

## Pitfalls

- approximate collision

## Related

- object-pool
