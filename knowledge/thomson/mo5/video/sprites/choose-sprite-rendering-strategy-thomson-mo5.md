# Choose sprite rendering strategy (Thomson MO5)

## Goal

Select the best rendering method depending on constraints.

## Decision tree

- Uniform background → use form-only
- Few sprites, simple scene → use background-preserving
- Many sprites or overlap → use dirty rectangle

## Trade-offs

| Mode | Speed | Quality | Complexity |
|------|------|--------|-----------|
| form-only | 🔥 fastest | low | simple |
| bg | medium | good | medium |
| dirty rect | slower | best | complex |

## Pitfalls

- using dirty rect too early (overkill)
- using form-only on complex backgrounds

## Related

- use-form-only-rendering
- use-dirty-rectangle-rendering
