# Use object pool to optimize entity processing (Thomson MO5)

## Goal

Efficiently manage entities without dynamic allocation.

## Concept

Use fixed-size arrays with an active flag.

## Example

```c
ActiveActor bullets[MAX];
```

## Benefits

- No malloc/free overhead
- Predictable memory usage
- Fast iteration

## Impact

- Reduces CPU overhead
- Works well with collision filtering

## When to use

- Bullets
- Enemies
- Effects

## Pitfalls

- Fixed capacity limit
- must handle full pool

## Related

- reduce-collision-tests-entity-filtering
- too-many-collision-tests-cause-on2-slowdown
