# Avoid float and double types on Thomson targets (CMOC, Thomson MO5)

Avoid using `float` and `double` when compiling for Thomson targets, because CMOC does not support floating-point arithmetic on those platforms.

## Steps

1. Use integer or fixed-point arithmetic instead of `float`/`double`.
2. If you need fractions, store scaled integers (e.g., `value * 256`) and shift when rendering.

## Notes

- CMOC supports floats only on CoCo/Dragon targets by relying on their BASIC routines.

Source: `knowledge/docs/cmoc_thomson_mo5.md`
