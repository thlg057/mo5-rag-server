# Schedule VRAM access during E=0 phase (Thomson MO5)

Avoid display contention by doing VRAM accesses when the CPU clock signal **E is low (E=0)**, because the gate array uses other phases for display refresh.

## Steps

1. Keep VRAM writes short and grouped.
2. Prefer block writes (byte loops) over scattered per-pixel writes.
3. When timing is critical (e.g., tight raster sync), align heavy VRAM work to safe periods instead of doing it continuously.

## Timing reference

- Frame (trame): ~20 ms (~50 Hz)
- Line: ~64 µs, with ~40 µs for active display

Source: `knowledge/docs/Guide de Programmation Technique.md`, `knowledge/docs/mo5_guide_synthese.md`
