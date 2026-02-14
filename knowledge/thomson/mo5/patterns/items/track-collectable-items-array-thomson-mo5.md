# Track collectable items with an array (Thomson MO5)

Store collectable items (coins, keys, etc.) in a fixed array and mark them as collected.

## Steps

1. Define an `Item` struct with position and an `active` flag.
2. Iterate items each frame:
   - if active and colliding with player, deactivate and increment score.
3. Render only active items.

Source: `knowledge/docs/mo5_game_dev_guide.md`
