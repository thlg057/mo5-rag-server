# Use object pool for entities (Thomson MO5)

Manage entities without dynamic allocation.

## Goal

Avoid malloc and ensure predictable memory usage.

## Concept

Preallocate fixed-size arrays and reuse slots.

## Example

```c
static Actor bullets[MAX_BULLETS];
```

## When to use

- Bullets
- Enemies
- Effects

## Benefits

- No fragmentation
- Fast allocation
- Deterministic behavior

## Pitfalls

- Fixed capacity limit
- Must track active state

## Related

- detect-collision-aabb-rectangles
- reduce-collision-tests

Source: gameplay doc
