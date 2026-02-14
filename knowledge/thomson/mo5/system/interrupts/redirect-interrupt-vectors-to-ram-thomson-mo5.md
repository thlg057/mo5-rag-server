# Redirect interrupt vectors to RAM jump slots (Thomson MO5)

Use the MO5 scheme where hardware vectors live in ROM (`$FFF0..$FFFF`) but point to indirection/jump slots in RAM, so a program can hook IRQ/FIRQ handlers.

## Steps

1. Identify the vector you want to override (IRQ, FIRQ, etc.).
2. Locate the corresponding RAM jump slot used by the ROM vector.
3. Write a jump instruction in RAM to your handler.
4. Ensure your handler preserves registers as required by your calling convention.

## Notes

- The exact RAM addresses for the jump slots depend on the system ROM conventions.
- Hooking interrupts is typically used for video synchronization and periodic updates.

Source: `knowledge/docs/mo5_guide_synthese.md`
