
# Compile C program using CMOC (Thomson MO5)

Compile a binary for MO5 using cmoc.

## Goal

Generate a .BIN executable compatible with MO5.

## Command

```bash
cmoc --thommo --org=0x2600 \
  -I./tools/include \
  -o bin/MYPROG.BIN \
  src/main.c tools/lib/libsdk_mo5.a
```

## Explanation

- --thommo → target MO5
- --org=0x2600 → load address
- include path → headers
- link with libsdk_mo5

## Common mistakes

- ❌ Missing library → linker errors
- ❌ Wrong org → crash at runtime

Source: guide-graphical-development-mo5.md
