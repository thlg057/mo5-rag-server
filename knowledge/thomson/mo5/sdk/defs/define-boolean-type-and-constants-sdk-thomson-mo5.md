# Définir le type booléen et les constantes de base du SDK MO5 (Thomson MO5)

Ce chunk résume le type `Boolean` et les constantes de base fournies par `mo5_defs.h`.

## Goal

- Savoir déclarer et utiliser le type booléen du SDK.
- Connaître les constantes de taille et de caractères les plus utiles.

## Type booléen `Boolean`

```c
typedef enum { FALSE = 0, TRUE = 1 } Boolean;
```

- `FALSE` vaut `0`, `TRUE` vaut `1`.
- Utilise ce type plutôt qu'un `int` nu lorsque tu veux représenter un booléen lisible.

## Constantes importantes

- `MO5_MAX_NAME_LENGTH = 30` : longueur maximale d'un nom (prévois un buffer de `MO5_MAX_NAME_LENGTH + 1` pour le `"\0"`).
- `MO5_BUFFER_SIZE = 32` : taille standard de buffer pour les petites saisies.
- `MO5_CLEAR_SCREEN = 12` : code de clear écran.
- `MO5_BACKSPACE_CHAR = 8` : retour arrière.
- `MO5_ENTER_CHAR = 13` : touche Entrée.
- `MO5_SPACE_CHAR = 32` : espace.
- `MO5_LINE_FEED = 10` : saut de ligne.

## Exemple

```c
#include "mo5_defs.h"

char name[MO5_MAX_NAME_LENGTH + 1];
Boolean done = FALSE;
```

## Notes

- `mo5_defs.h` est le header de base du SDK ; `mo5_stdio.h` et `mo5_ctype.h` en dépendent.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_defs_h.md`
