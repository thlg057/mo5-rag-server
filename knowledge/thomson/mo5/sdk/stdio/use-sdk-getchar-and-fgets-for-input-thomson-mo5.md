# Utiliser `getchar` et `fgets` du SDK MO5 pour la saisie texte (Thomson MO5)

Ce chunk décrit l'alias `getchar` et la fonction `fgets` fournis par `mo5_stdio.h` pour lire du texte au clavier.

## Goal

- Lire un caractère au clavier avec le SDK.
- Lire une ligne de texte dans un buffer avec gestion de backspace et écho.

## Inclusion et dépendance

```c
#include "mo5_stdio.h"   // inclut "mo5_defs.h"
```

## Macro `getchar`

```c
#define getchar mo5_getchar
```

- Alias direct vers `mo5_getchar()` défini dans `mo5_defs.h`.
- Comportement **non bloquant** : retourne `0` s'il n'y a pas de touche.
- Pour une lecture bloquante, boucle jusqu'à un retour non nul ou utilise `wait_for_key()`.

## Lire une chaîne avec `fgets`

```c
int fgets(char *buffer, int max_length);
```

- Lit une ligne de texte saisie au clavier dans `buffer`.
- `max_length` : nombre **maximal de caractères utiles** (le buffer doit faire au moins `max_length + 1` pour le `"\0"`).
- Retourne le nombre de caractères effectivement lus.
- Termine sur pression de `MO5_ENTER_CHAR`.
- Gère le backspace (`MO5_BACKSPACE_CHAR`) en effaçant caractère et affichage.
- Affiche automatiquement chaque caractère imprimable saisi (utilise `isprint()` de `mo5_ctype.h`).

## Exemple

```c
char buf[MO5_BUFFER_SIZE];
clrscr();
puts("Entrez votre nom :");
fgets(buf, MO5_BUFFER_SIZE);
```

## Notes

- `fgets` utilise en interne `mo5_getchar()` en boucle et fournit donc une lecture **bloquante** de plus haut niveau.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_stdio_h.md`
