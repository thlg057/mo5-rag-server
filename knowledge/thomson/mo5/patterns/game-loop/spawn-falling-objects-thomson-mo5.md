# Spawn falling objects with a fixed pool (Thomson MO5)

Create falling objects (drops, meteors) using a fixed-size pool to avoid dynamic allocation.

## Steps

1. Allocate an array of objects with an `active` flag.
2. To spawn: find an inactive slot, set `active=1`, initialize `(x,y,vy)`.
3. To update: `y += vy`, deactivate when off-screen.

Source: `knowledge/docs/mo5_game_dev_guide.md`
