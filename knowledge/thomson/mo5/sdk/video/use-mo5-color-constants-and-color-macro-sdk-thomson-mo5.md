# Utiliser les constantes de couleurs et la macro `COLOR` du SDK MO5 (Thomson MO5)

Ce chunk décrit la palette 16 couleurs et la macro `COLOR` définies dans `mo5_video.h`.

## Goal

- Connaître les constantes de couleurs du MO5.
- Construire un octet couleur `FFFFBBBB` avec `COLOR(bg, fg)`.

## Palette 16 couleurs

`mo5_video.h` expose des constantes symboliques :

- `C_BLACK`, `C_RED`, `C_GREEN`, `C_YELLOW`, `C_BLUE`, `C_MAGENTA`, `C_CYAN`, `C_WHITE`
- `C_GRAY`, `C_LIGHT_RED`, `C_LIGHT_GREEN`, `C_LIGHT_YELLOW`, `C_LIGHT_BLUE`, `C_PURPLE`, `C_LIGHT_CYAN`, `C_ORANGE`

Chaque constante représente une valeur 0–15 interprétée par le hardware.

## Macro `COLOR(bg, fg)`

```c
#define COLOR(bg, fg)  /* FFFFBBBB : fg dans les bits 4–7, bg dans les bits 0–3 */
```

- `bg` : couleur de fond (bits 0–3).
- `fg` : couleur de forme (bits 4–7).

Exemples :

```c
unsigned char black_on_white = COLOR(C_BLACK,  C_WHITE);
unsigned char yellow_on_blue = COLOR(C_BLUE,   C_YELLOW);
unsigned char all_black      = COLOR(C_BLACK,  C_BLACK);
```

## Notes

- Utilise `COLOR` pour toutes les écritures dans la banque couleur (remplissage écran, sprites, HUD...).

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_video_h.md`
