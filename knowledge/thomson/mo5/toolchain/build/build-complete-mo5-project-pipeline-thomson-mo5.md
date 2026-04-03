# Build complete MO5 project pipeline (Thomson MO5)

Full workflow from source code to runnable disk.

## Goal

Produce executable disk image for emulator or real hardware.

## Steps

1. Write code (CMOC)
2. Compile → .BIN
3. Link SDK
4. Generate .fd disk
5. Convert to .sd if needed
6. Run in emulator

## Example

```bash
make install
make
```

## When to use

- Any MO5 project
- Game development workflow

## Pitfalls

- Missing SDK install
- Wrong include paths
- Forgetting disk generation

## Advanced

- Integrate asset pipeline (PNG → sprite)
- Automate conversion steps

## Related

- convert-fd-to-sd-disk-image
- setup-mo5-development-environment

Source: template
