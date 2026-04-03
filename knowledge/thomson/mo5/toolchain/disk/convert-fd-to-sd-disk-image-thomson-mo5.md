# Convert .fd to .sd disk image (Thomson MO5)

Convert a 720KB floppy image into 320KB format.

## Goal

Make disk compatible with MO5/TO7 systems.

## Concept

- .fd → 720KB (3.5")
- .sd → 320KB (5.25")
- 256-byte sectors

## Algorithm

1. Read 256-byte sector
2. Write sector
3. Write padding (empty bytes)
4. Repeat for all tracks
5. Fill missing sectors with 0xFF

## Example logic

```python
data = fd.read(256)
sd.write(data)
sd.write(padding)
```

## Details from implementation

- 4 faces × 80 tracks
- 16 sectors per track
- missing sectors → fill with 0xFF

Source: fd2sd.py fileciteturn11file0

## When to use

- Real hardware compatibility

## Pitfalls

- Incorrect sector size
- Corrupt input file
