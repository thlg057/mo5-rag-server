# Use dirty rectangle rendering (Thomson MO5)

## Goal

Handle overlapping sprites efficiently.

## Concept

Save → restore → redraw.

## Example

```c
save();
restore();
draw();
```

## When to use

- overlapping sprites

## Pitfalls

- memory overhead

## Related

- form-only-rendering
