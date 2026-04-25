# Estimate CPU Cycle Budget per Frame (Thomson MO5)

Référence des coûts en cycles pour les opérations courantes. À 1 MHz, le budget total est ~20 000 cycles par frame (50 Hz PAL).

## Coût des opérations sprite

| Opération | Coût approx. | Notes |
|-----------|-------------|-------|
| `draw_sprite` 8×8 | ~50 cycles | |
| `clear_sprite` 8×8 | ~65 cycles | |
| `draw_sprite` 16×16 | ~200 cycles | |
| `clear_sprite` 16×16 | ~200 cycles | |
| `draw_sprite` 24×24 | ~450 cycles | |
| Switch banque VRAM | ~5 cycles | Par switch |
| Accès `row_offsets[y]` | ~8 cycles | Vs ~30+ pour `y*40` |
| Collision AABB | ~20 cycles | 4 comparaisons |
| Lecture touche PIA | ~10–20 cycles | |
| `mo5_font6_puts` (1 car.) | ~30–50 cycles | |
| LFSR pseudo-random | ~15 cycles | |

## Formule d'estimation sprite

```
coût ≈ (W_bytes × H × 5) + (H × 3)
```

Exemple sprite 4×32 : `(4 × 32 × 5) + (32 × 3) = 736 cycles`

> Valeurs théoriques — mesurer sur émulateur pour valeurs précises.

## Budget type — jeu avec 4 ennemis

| Opération | Coût estimé |
|-----------|------------|
| Joueur `move_sprite` 16×16 | ~200 cycles |
| 4 ennemis `move_sprite` 16×16 | ~800 cycles |
| 3 tirs joueur `move_sprite` 8×8 | ~150 cycles |
| 4 tirs ennemis `move_sprite` 8×8 | ~200 cycles |
| Collisions (paires catégorielles) | ~240 cycles |
| Score + vies (font6) | ~400 cycles |
| **Total estimé** | **~2 000 cycles** |

Ce budget dépasse la fenêtre VBL seule (~1 200 cycles).
Utiliser la stratégie A (20ms complètes) ou `move_sprite` optimisé (clear+draw fusionnés).

## Notes

- Fenêtre VBL : ~1 200 cycles seulement
- Frame complète : ~20 000 cycles
- Les collisions s'exécutent pendant le balayage actif — pas dans le budget VBL

Source: `mo5_hardware_reference.md` section 4
