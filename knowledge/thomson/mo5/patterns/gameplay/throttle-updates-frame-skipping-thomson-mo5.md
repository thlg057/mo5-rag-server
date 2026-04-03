# Throttle updates using frame skipping (Thomson MO5)

## Goal

Reduce CPU usage.

## Example

```c
tick++;
if (tick == SPEED) {
    update();
    tick = 0;
}
```

## Why

Not all objects need 50Hz updates.

## Pitfalls

- too slow → unresponsive gameplay
