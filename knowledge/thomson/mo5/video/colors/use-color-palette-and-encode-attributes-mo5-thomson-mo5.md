# Use Color Palette and Encode Attributes (Thomson MO5)

Le MO5 dispose de 16 couleurs fixes. Chaque octet de la banque COULEUR encode fond et forme en format `FFFFBBBB`.

Source : Manuel Technique MO5 p.13 + Clefs Pour MO5 p.140 — Fiabilité ÉLEVÉE.

## Palette complète

| Index | Constante SDK | Couleur | Codage (R,V,B,demi) |
|-------|--------------|---------|----------------------|
| 0 | `C_BLACK` | Noir | 0000 |
| 1 | `C_RED` | Rouge | 0001 |
| 2 | `C_GREEN` | Vert | 0010 |
| 3 | `C_YELLOW` | Jaune | 0011 |
| 4 | `C_BLUE` | Bleu marine | 0100 |
| 5 | `C_MAGENTA` | Magenta | 0101 |
| 6 | `C_CYAN` | Cyan | 0110 |
| 7 | `C_WHITE` | Blanc | 0111 |
| 8 | `C_GRAY` | Gris | 1000 |
| 9 | `C_LIGHT_RED` | Rouge pâle | 1001 |
| 10 | `C_LIGHT_GREEN` | Vert pâle | 1010 |
| 11 | `C_LIGHT_YELLOW` | Jaune pâle | 1011 |
| 12 | `C_LIGHT_BLUE` | Bleu clair | 1100 |
| 13 | `C_PURPLE` | Magenta pâle | 1101 |
| 14 | `C_LIGHT_CYAN` | Cyan pâle | 1110 |
| 15 | `C_ORANGE` | **Orange** | 1111 |

> ⚠️ Couleur 15 = Orange (exception — `1111` devrait logiquement être blanc clair).
> Le codage passe par une PROM analogique, pas un mapping RGB linéaire.

## Encodage d'un octet couleur VRAM

```
bit 7–4 : couleur FORME (fg)
bit 3–0 : couleur FOND  (bg)
```

## Macro COLOR

```c
#define COLOR(bg, fg)  /* construit l'octet FFFFBBBB */
```

```c
COLOR(C_BLACK, C_WHITE)    /* 0x70 — blanc sur noir */
COLOR(C_BLUE,  C_YELLOW)   /* 0x34 — jaune sur bleu */
COLOR(C_BLACK, C_BLACK)    /* 0x00 — tout noir */
COLOR(C_BLACK, C_RED)      /* 0x10 — rouge sur noir */
```

## Contrainte hardware — color clash

Un octet couleur couvre **8 pixels horizontaux**. Il est impossible d'avoir plus de 2 couleurs dans ce groupe. Toute conception graphique doit respecter cette contrainte.

## Erreurs fréquentes

```text
❌ Inverser bg et fg dans COLOR() → couleurs inversées
❌ Supposer un mapping RGB linéaire → couleurs incorrectes
❌ Plus de 2 couleurs dans un groupe de 8px → color clash hardware
```

Source: `mo5_hardware_reference.md` section 5
