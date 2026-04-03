# Reduce collision tests using entity filtering (Thomson MO5)

## Goal

Minimize number of collision checks.

## Concept

Only test relevant and active entities.

## Techniques

- Skip inactive objects
- Separate entity types (bullets vs enemies)
- Early exit when collision found

## Example

```c
if (!bullet.active) continue;
```

## Impact

- Can reduce collision checks by 50–90%
- Improves frame stability

## When to use

- Any game with multiple entities

## Pitfalls

- forgetting active flags
- mixing unrelated entity types

## Related

- too-many-collision-tests-cause-on2-slowdown
- use-object-pool-for-entities
