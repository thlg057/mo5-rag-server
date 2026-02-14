# Update patrolling enemy with left-right direction (Thomson MO5)

Implement an enemy that moves left/right and reverses direction on boundaries.

## Steps

1. Store `x` and `dx` (+1 or -1).
2. Each frame: `x += dx`.
3. If `x` hits a boundary, negate `dx`.

Source: `knowledge/docs/mo5_game_dev_guide.md`
