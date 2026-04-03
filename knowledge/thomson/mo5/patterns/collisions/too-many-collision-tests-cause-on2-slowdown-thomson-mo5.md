# Too many collision tests cause O(n²) slowdown (Thomson MO5)

## Goal

Explain why collision systems become slow with many entities.

## Problem

Each entity is tested against many others.

Example:
- 10 enemies × 5 bullets = 50 tests per frame

## Concept

This leads to O(n²) complexity.

On a 6809 CPU (~20k cycles/frame), this becomes expensive quickly.

## Impact

- CPU overload
- frame drops
- input lag

## Solution

- Filter inactive entities
- Reduce candidate pairs
- Group entities logically (bullet vs enemy only)

## When to use

- Games with multiple enemies and projectiles

## Pitfalls

- testing all entities blindly
- forgetting early exit

## Related

- detect-collision-aabb
- reduce-collision-tests-entity-filtering
- use-object-pool-for-entities
