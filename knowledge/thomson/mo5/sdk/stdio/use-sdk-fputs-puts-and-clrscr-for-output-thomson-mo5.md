# Utiliser `fputs`, `puts` et `clrscr` du SDK MO5 pour l'affichage texte (Thomson MO5)

Ce chunk présente les fonctions de sortie texte de `mo5_stdio.h` : écriture de chaînes et effacement de l'écran.

## Goal

- Afficher une chaîne avec ou sans saut de ligne.
- Effacer l'écran proprement.

## Inclusion

```c
#include "mo5_stdio.h"
```

## Afficher des chaînes

```c
void fputs(const char *s);
void puts(const char *s);
```

- `fputs(s)` : écrit `s` caractère par caractère via `putchar` (alias `mo5_putchar`), **sans** saut de ligne.
- `puts(s)` : appelle `fputs(s)` puis `mo5_newline()` pour ajouter un CR+LF.

Utilise ces fonctions pour tout affichage texte simple (menus, HUD texte, messages debug).

## Effacer l'écran : `clrscr`

```c
void clrscr(void);
```

- Envoie `MO5_CLEAR_SCREEN` (code 12) via `putchar`.
- Efface entièrement l'écran et repositionne le curseur.

## Exemple

```c
clrscr();
puts("Bienvenue sur MO5 !");
fputs("Score : ");
```

## Notes

- Toute la couche texte du SDK repose sur `mo5_putchar` défini dans `mo5_defs.h`.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_stdio_h.md`
