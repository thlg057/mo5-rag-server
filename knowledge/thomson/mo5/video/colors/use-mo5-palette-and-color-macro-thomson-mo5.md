# Utiliser la palette MO5 et la macro `COLOR`

## Goal

Documenter les constantes de palette et la macro `COLOR(bg, fg)` utilisées par le SDK sprites MO5.

## Palette MO5 (constantes)

```c
// Exemples de constantes de couleur
C_BLACK, C_RED, C_GREEN, C_YELLOW,
C_BLUE,  C_MAGENTA, C_CYAN,  C_WHITE,
C_GRAY,  C_LIGHT_RED, C_LIGHT_GREEN, C_LIGHT_YELLOW,
C_LIGHT_BLUE, C_PURPLE, C_LIGHT_CYAN, C_ORANGE;
```

Résumé (nom → index) :

| Constante       | Valeur | Couleur       |
|-----------------|--------|---------------|
| `C_BLACK`       | 0      | Noir          |
| `C_WHITE`       | 7      | Blanc         |
| `C_BLUE`        | 4      | Bleu          |
| `C_YELLOW`      | 3      | Jaune         |
| `C_RED`         | 1      | Rouge         |
| `C_GREEN`       | 2      | Vert          |
| `C_ORANGE`      | 15     | Orange        |
| ...             | ...    | autres couleurs |

## Macro `COLOR(bg, fg)`

Sur MO5, un octet de couleur encode **fond et forme** au format `FFFFBBBB` :

```c
#define COLOR(bg, fg)   /* bg = couleur fond, fg = couleur forme */
```

Exemples d’utilisation :

```c
unsigned char attr1 = COLOR(C_BLACK, C_WHITE);  // texte blanc sur fond noir
unsigned char attr2 = COLOR(C_BLUE,  C_YELLOW); // jaune sur bleu
```

Ces attributs servent à :
- Initialiser le fond (via `mo5_init_graphic_mode(attr)`).
- Remplir les buffers `color` des sprites.

Source: `mo5-docs/mo5/mo5_sprite.md`

