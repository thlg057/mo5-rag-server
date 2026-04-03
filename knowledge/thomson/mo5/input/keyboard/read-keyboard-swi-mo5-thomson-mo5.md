# Read keyboard using SWI (Thomson MO5)

Use system interrupt to read key input.

## Concept

Keyboard input uses SWI call.

## Example

```c
asm {
    swi
    fcb $0A
}
```

## Pitfalls

- May return 0 if no key
- must filter input

## Related

- ignore-null-keypress
