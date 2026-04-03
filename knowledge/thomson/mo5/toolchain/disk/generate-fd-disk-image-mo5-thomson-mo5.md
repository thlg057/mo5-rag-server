# Generate .fd disk image (Thomson MO5)

Create a bootable floppy disk image from a compiled program.

## Goal

Produce a disk image runnable on emulator or real hardware.

## Concept

A .fd image is a 3.5" floppy (720KB) with:
- tracks
- sectors (256 bytes)
- boot sector + program

## Procedure

1. Compile program → .BIN
2. Insert binary into disk structure
3. Build bootable image

## Implementation strategy

- Create empty disk buffer
- Write boot sector
- Copy program into sectors
- Ensure correct sector ordering

## When to use

- Always before running program
- Emulator or real disk

## Pitfalls

- Wrong sector size (must be 256 bytes)
- Incorrect track ordering
- Missing boot sector

## Related

- convert-fd-to-sd-disk-image
