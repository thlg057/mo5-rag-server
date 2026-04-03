# Read keyboard pattern (Thomson MO5)

## Example

```c
do {
    ch = getchar();
} while (ch == 0);
```

## Why

0 means no key.

## Pitfalls

- forgetting loop → missed input
