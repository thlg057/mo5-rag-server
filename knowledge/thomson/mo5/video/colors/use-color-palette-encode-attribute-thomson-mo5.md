# Use Color Palette and Encode Color Attribute (Thomson MO5)

Le MO5 dispose de 16 couleurs codées sur 4 bits (RVB + intensité). Chaque octet de la banque COULEUR encode deux couleurs : fond (bits 0–3) et forme (bits 4–7).

## Palette (fiabilité MOYENNE — vérifier sur vrai hardware)

| Code | Couleur | Constante SDK |
|------|---------|---------------|
| 0 | Noir | `C_BLACK` |
| 1 | Rouge | `C_RED` |
| 2 | Vert | `C_GREEN` |
| 3 | Jaune | `C_YELLOW` |
| 4 | Bleu | `C_BLUE` |
| 5 | Magenta | `C_MAGENTA` |
| 6 | Cyan | `C_CYAN` |
| 7 | Blanc | `C_WHITE` |
| 8 | Gris | `C_GRAY` |
| 9 | Rose | `C_LIGHT_RED` |
| 10 | Vert clair | `C_LIGHT_GREEN` |
| 11 | Jaune clair | `C_LIGHT_YELLOW` |
| 12 | Bleu clair | `C_LIGHT_BLUE` |
| 13 | Violet | `C_PURPLE` |
| 14 | Cyan clair | `C_LIGHT_CYAN` |
| 15 | Orange | `C_ORANGE` |

## Format d'un octet couleur VRAM

```
bit 7–4 : couleur FORME  (fg)
bit 3–0 : couleur FOND   (bg)
```

## Macro COLOR

```c
#define COLOR(bg, fg)   /* construit l'octet FFFFBBBB */

/* Exemples */
COLOR(C_BLACK, C_WHITE)    /* texte blanc sur fond noir  → 0x70 */
COLOR(C_BLUE,  C_YELLOW)   /* jaune sur bleu             → 0x34 */
COLOR(C_BLACK, C_BLACK)    /* écran noir (effacement)    → 0x00 */
```

## Contrainte hardware

Chaque octet de la VRAM définit 2 couleurs pour un groupe de **8 pixels horizontaux**.
Il est impossible d'avoir plus de 2 couleurs différentes dans un même groupe de 8 pixels.
Conception graphique obligatoirement alignée sur cette contrainte (color clash).

## Notes

- Le mapping couleur passe par une PROM analogique — pas linéaire RGB standard
- Ne pas supposer d'équivalence avec une palette RGB quelconque
- Utiliser des tables prédéfinies testées sur émulateur ou vrai hardware

Source: `mo5_hardware_reference.md` section 5
