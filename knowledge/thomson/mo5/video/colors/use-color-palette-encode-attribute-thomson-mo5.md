# Use Color Palette and Encode Color Attribute (Thomson MO5)

Le MO5 dispose de 16 couleurs codées sur 4 bits (RVB + intensité). Chaque octet de la banque COULEUR encode deux couleurs : fond (bits 0–3) et forme (bits 4–7).

## Palette officielle — codage RVB+demi-teinte

Source : Manuel Technique MO5 p.13 + Clefs Pour MO5 p.140 + Guide du MO5 p.249 — Fiabilité ÉLEVÉE.

Le codage est **physique** (4 bits : Rouge, Vert, Bleu, demi-teinte) via une PROM analogique.

| Code | Couleur | Binaire (R,V,B,demi) | Constante SDK |
|------|---------|----------------------|---------------|
| 0 | Noir | 0000 | `C_BLACK` |
| 1 | Rouge | 0001 | `C_RED` |
| 2 | Vert | 0010 | `C_GREEN` |
| 3 | Jaune | 0011 | `C_YELLOW` |
| 4 | Bleu marine | 0100 | `C_BLUE` |
| 5 | Magenta | 0101 | `C_MAGENTA` |
| 6 | Cyan | 0110 | `C_CYAN` |
| 7 | Blanc | 0111 | `C_WHITE` |
| 8 | Gris | 1000 | `C_GRAY` |
| 9 | Rouge pâle | 1001 | `C_LIGHT_RED` |
| 10 | Vert pâle | 1010 | `C_LIGHT_GREEN` |
| 11 | Jaune pâle | 1011 | `C_LIGHT_YELLOW` |
| 12 | Bleu clair | 1100 | `C_LIGHT_BLUE` |
| 13 | Magenta pâle | 1101 | `C_PURPLE` |
| 14 | Cyan pâle | 1110 | `C_LIGHT_CYAN` |
| 15 | **Orange** | 1111 | `C_ORANGE` |

Codage des 4 bits (du LSB au MSB) :
- bit 0 : présence de **rouge**
- bit 1 : présence de **vert**
- bit 2 : présence de **bleu**
- bit 3 : **demi-teinte** (intensité) — exception : `1111` = Orange au lieu de Blanc clair

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

Source: `mo5_hardware_reference.md` section 5 — Fiabilité ÉLEVÉE (3 sources confirmées)
