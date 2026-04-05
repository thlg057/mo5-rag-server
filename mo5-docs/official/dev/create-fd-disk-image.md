# Creating a Floppy Disk Image (.fd) for the Thomson MO5

## Overview

This guide explains how to generate a bootable floppy disk image (`.fd`) from a compiled MO5 binary, using **BootFloppyDisk** by Olivier P.

The `.fd` format is a raw floppy disk image compatible with MO5 emulators such as DCMOTO. It combines a MO5 boot sector (`BOOTMO.BIN`) with your program binary into a single self-bootable image.

The process has two steps:
1. Install and compile the BootFloppyDisk toolchain
2. Use the `fdfs` tool to generate the `.fd` image

---

## Prerequisites

- `git`
- `make`
- A compiled MO5 binary (`.BIN` file)

---

## Step 1 — Install the BootFloppyDisk toolchain

Clone and compile the BootFloppyDisk project by Olivier P:

```bash
git clone https://github.com/OlivierP-To8/BootFloppyDisk.git
make -C BootFloppyDisk
```

This produces two artifacts inside the cloned repository:

- `BootFloppyDisk/tools/fdfs` — the floppy image creation tool
- `BootFloppyDisk/BOOTMO.BIN` — the MO5 boot sector binary

---

## Step 2 — Generate the .fd image

Use the `fdfs` tool with the `-addBL` flag (Add Boot Loader):

```bash
BootFloppyDisk/tools/fdfs -addBL <output>.fd BootFloppyDisk/BOOTMO.BIN <your_program>.BIN
```

**Example** — generating `SPRITE.fd` from `SPRITE.BIN`:

```bash
BootFloppyDisk/tools/fdfs -addBL SPRITE.fd BootFloppyDisk/BOOTMO.BIN SPRITE.BIN
```

### How it works

The `-addBL` flag instructs `fdfs` to inject the boot loader (`BOOTMO.BIN`) into the first sector of the disk image, then append your program binary. The resulting `.fd` file is directly bootable on a MO5 or in an emulator.

---

## Troubleshooting

**`fdfs: Permission denied` or `not found`**
Make sure the toolchain compiled successfully. Re-run `make -C BootFloppyDisk` and check for errors, particularly missing dependencies (`flex`, `lwtools`).

**Blank screen or boot failure in emulator**
Verify that your `.BIN` file is a valid MO5 binary compiled with the correct origin address (`--org=0x2600` with cmoc). A binary compiled for the wrong address will not boot correctly.

**`BOOTMO.BIN` missing**
The BootFloppyDisk build did not complete. Check that all dependencies are installed and run `make -C BootFloppyDisk` again.

---

## References

- BootFloppyDisk repository: https://github.com/OlivierP-To8/BootFloppyDisk
- Related guides: *Compiling a MO5 program with cmoc*, *Converting a .fd image to .sd*
