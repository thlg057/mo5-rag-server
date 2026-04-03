# Implement game loop with VBL (Thomson MO5)

## Goal

Synchronize rendering with screen refresh.

## Pattern

```c
while (1) {
    input();
    update();
    mo5_wait_vbl();
    render();
}
```

## Why

- avoids tearing
- maximizes CPU usage

## Pitfalls

- rendering before VBL → flicker
- heavy logic after VBL → frame drop
