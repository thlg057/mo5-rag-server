# Map RAM and ROM address space (Thomson MO5)

Use the MO5 main memory map to place code/data safely and to reason about what the BASIC/monitor occupies.

## Steps

1. Treat `$0000..$7FFF` as the main RAM area described as “user RAM” in many MO5 summaries.
2. Treat `$C000..$FFFF` as ROM (monitor + BASIC).
3. When you place your code/data, ensure you do not overlap ROM-mapped regions.

## Notes

- Many MO5 programming workflows also treat `$0000..$1F3F` as a VRAM window (banked) when graphics is enabled.
- If you use a toolchain (e.g., CMOC), align this map with your chosen `org` / load address.

Source: `knowledge/docs/Guide de Programmation Technique.md`, `knowledge/docs/mo5_guide_synthese.md`
