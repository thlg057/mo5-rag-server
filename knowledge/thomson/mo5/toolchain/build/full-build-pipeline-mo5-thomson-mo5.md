# Full build pipeline (Thomson MO5)

## Steps

1. Write C code
2. Compile with CMOC → .BIN
3. Link SDK
4. Generate .fd disk image
5. Convert to .sd if needed
6. Run in emulator

## Pitfalls

- missing includes
- wrong output path
