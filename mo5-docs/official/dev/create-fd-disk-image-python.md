# Creating a Floppy Disk Image (.fd) for the Thomson MO5

## Overview

This guide explains how to generate a bootable floppy disk image (`.fd`) from a compiled MO5 binary, using the `makefd.py` script included in the **sdk_mo5** repository.

The `.fd` format is a raw floppy disk image compatible with MO5 emulators such as DCMOTO. It combines a MO5 boot sector with your program binary into a single self-bootable image.

The MO5 boot loader is embedded directly inside `makefd.py` — no external toolchain or additional repository is required.

---

## Prerequisites

- Python 3 (any recent version)
- No additional Python packages required — the script uses only the standard library
- A compiled MO5 binary (`.BIN` file with Thomson header, e.g. produced by cmoc with `--thommo`)

---

## Step 1 — Install the sdk_mo5 repository

The `makefd.py` script is distributed as part of the sdk_mo5 project. Clone the repository to get it:

```bash
git clone https://github.com/thlg057/sdk_mo5.git
```

The script is located at:

```
sdk_mo5/scripts/makefd.py
```

---

## Step 2 — Generate the .fd image

```bash
python3 sdk_mo5/scripts/makefd.py <output>.fd <your_program>.BIN
```

**Example** — generating `SPRITE.fd` from `SPRITE.BIN`:

```bash
python3 sdk_mo5/scripts/makefd.py SPRITE.fd SPRITE.BIN
```

The script accepts the following arguments:

| Argument          | Description                            |
|-------------------|----------------------------------------|
| `<output>.fd`     | Path for the generated `.fd` image     |
| `<program>.BIN`   | Your compiled MO5 binary (one or more) |

Multiple binaries can be passed — they will all be written to the same disk image:

```bash
python3 sdk_mo5/scripts/makefd.py SPRITE.fd SPRITE.BIN ASSETS.BIN
```

The `.fd` file is written directly to the path you specify, regardless of the current working directory.

---

## How it works

`makefd.py` creates a 327 680-byte raw disk image matching the Thomson MO5 floppy geometry (80 tracks × 16 sectors × 256 bytes). It:

1. Formats the image (FAT, directory, disk name)
2. Writes your `.BIN` file(s) into the Thomson filesystem
3. Injects the MO5 boot loader into the first sector, with the file location descriptors needed for the ROM to load and execute your program at boot

The boot loader binary (`BOOTMO.BIN`, originally from the BootFloppyDisk project by Olivier P) is embedded directly in the script — no compilation step is needed.

---

## Troubleshooting

**Boot failure or blank screen in emulator**
Verify that your `.BIN` file is a valid MO5 binary with a Thomson header, compiled with the correct origin address (`--org=0x2600` with cmoc and `--thommo`). A binary compiled for the wrong address will not boot correctly.

**`FileNotFoundError`**
Check that the path to your `.BIN` file is correct and that the output directory exists. The script will not create missing intermediate directories.

---

## References

- sdk_mo5 repository: https://github.com/thlg057/sdk_mo5
- BootFloppyDisk (original toolchain): https://github.com/OlivierP-To8/BootFloppyDisk
- Related guides: *Compiling a MO5 program with cmoc*, *Converting a .fd image to .sd*
