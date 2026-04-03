# Ignore null keypress (Thomson MO5)

## Problem

Keyboard read may return 0.

## Solution

Loop until non-zero value.

## Example

```c
do {
    ch = mo5_getchar();
} while (ch == 0);
```

## Why

0 means no key pressed.

## Related

- read-keyboard-swi
