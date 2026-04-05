# Converting a Floppy Disk Image (.fd to .sd) for the SDDrive

## Overview

The **SDDrive** is an external peripheral for the Thomson MO5 that simulates a floppy disk drive from a file stored on an SD card. It expects disk images in the `.sd` format, which differs structurally from the `.fd` format produced by the standard build pipeline.

This guide explains how to convert a `.fd` image to `.sd` using the `fd2sd.py` script included in the **sdk_mo5** repository.

---

## Part 1 — Using fd2sd.py from sdk_mo5

### Installing the sdk_mo5 repository

The `fd2sd.py` script is distributed as part of the sdk_mo5 project. Clone the repository to get it:

```bash
git clone https://github.com/thlg057/sdk_mo5.git
```

The script is located at:

```
sdk_mo5/scripts/fd2sd.py
```

### Prerequisites

- Python 3 (any recent version)
- No additional Python packages required — the script uses only the standard library

### Running the conversion

```bash
python3 sdk_mo5/scripts/fd2sd.py -conv <input>.fd <output>.sd
```

**Example** — converting `SPRITE.fd` to `SPRITE.sd`:

```bash
python3 sdk_mo5/scripts/fd2sd.py -conv SPRITE.fd SPRITE.sd
```

The script accepts exactly three arguments:

| Argument   | Description                        |
|------------|------------------------------------|
| `-conv`    | Required flag to trigger conversion |
| `<input>.fd`  | Path to the source `.fd` image  |
| `<output>.sd` | Path for the generated `.sd` file |

On success, the script prints:

```
✓ Conversion réussie: SPRITE.sd
```

Copy the resulting `.sd` file to your SD card and load it via the SDDrive interface on your MO5.

---

## Part 2 — How the conversion works

### The two formats

| Property        | .fd                        | .sd                        |
|-----------------|----------------------------|----------------------------|
| Physical origin | 3.5" HD floppy (720 KB)    | 5.25" SD floppy (320 KB)   |
| Sector size     | 256 bytes                  | 512 bytes (256 data + 256 padding) |
| Geometry        | 2 sides × 80 tracks × 9 sectors | 4 faces × 80 tracks × 16 sectors |
| Total sectors   | 1440                       | 5120                       |

The MO5 originally used 5.25" single-density drives. The `.sd` format preserves this geometry, with each 256-byte sector padded to 512 bytes to align with the SDDrive's internal block layout.

### Conversion algorithm

The script iterates over **4 × 80 = 320 tracks**, and for each track over **16 sectors**:

```python
for track in range(4 * 80):
    for sector in range(1, 17):
        data = fd.read(256)
        if len(data) == 256:
            sd.write(data)        # 256 bytes of actual data
            sd.write(empty[:256]) # 256 bytes of 0xFF padding
        else:
            sd.write(empty)       # missing sector → full 0xFF block
```

Each 256-byte sector read from the `.fd` file is written followed by 256 bytes of `0xFF` padding, producing a 512-byte block in the `.sd` file. This doubling accounts for the difference in physical density between the original 5.25" SD media and modern storage.

If the `.fd` file is shorter than expected (truncated or incomplete), missing sectors are filled entirely with `0xFF` — the standard convention for unformatted or absent sectors on Thomson media.

### Data integrity

No data is transformed or reinterpreted during conversion — the sector content is copied verbatim. The only operation is structural: repackaging 256-byte sectors into 512-byte padded blocks, and remapping the geometry from the 3.5" HD layout to the 5.25" SD layout expected by the SDDrive.

### Output size

A valid `.sd` file is always exactly:

```
320 tracks × 16 sectors × 512 bytes = 2 621 440 bytes (2.5 MB)
```

---

## References

- sdk_mo5 repository: https://github.com/thlg057/sdk_mo5
- SDDrive project: search for "SDDrive Thomson" for hardware documentation
- Related guides: *Creating a .fd floppy disk image*, *Compiling a MO5 program with cmoc*
