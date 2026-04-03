# Detect collision using AABB rectangles (Thomson MO5)

Fast rectangle-based collision detection suitable for 6809 CPU constraints.

## Goal

Minimize CPU cost while keeping acceptable gameplay precision.

## Concept

Two rectangles collide if they overlap on both X and Y axes.

## Example

```c
(ax < bx + bw) && (ax + aw > bx) &&
(ay < by + bh) && (ay + ah > by);
```

## When to use

- All real-time games
- Bullet / enemy collisions
- Player vs environment

## When NOT to use

- Pixel-perfect collision (too expensive on 6809)

## Performance

~20 cycles per test

## Optimization tips

- Filter entities before testing
- Skip inactive objects
- Use early exit

## Pitfalls

- Bounding boxes may feel "too large"
- Adjust hitbox smaller than sprite

## Related

- reduce-collision-tests-entity-filtering
- use-object-pool-for-entities

Source: collisions_mo5.md
