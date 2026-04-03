# Convert .fd disk image to .sd (Thomson MO5)

Convert modern disk format into legacy compatible format.

## Goal

Run programs on MO5/TO7 real hardware.

## Concept

- .fd = 720KB (3.5")
- .sd = 320KB (5.25")

Conversion restructures tracks and sectors.

## Procedure

```bash
python3 fd2sd.py -conv disk.fd disk.sd
```

## Internal logic

- Read 256-byte sectors
- Duplicate/pad data
- Fill missing sectors with 0xFF

## When to use

- Real hardware testing
- Emulators requiring .sd

## Pitfalls

- Wrong input size
- Corrupted sectors

## Related

- build-complete-mo5-project-pipeline

Source: fd2sd.py
