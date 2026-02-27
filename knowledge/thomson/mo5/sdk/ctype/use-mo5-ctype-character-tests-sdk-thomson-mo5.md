# Utiliser les fonctions de classification de caractères `mo5_ctype` (Thomson MO5)

Ce chunk résume les fonctions de test de caractères ajoutées par `mo5_ctype.h` : `islower`, `isupper`, `isprint`, `ispunct`.

## Goal

- Tester le type d'un caractère (lettre, ponctuation, imprimable) avec le SDK MO5.
- Combiner ces fonctions avec celles déjà fournies par CMOC.

## Inclusion

```c
#include "mo5_ctype.h"  // inclut "mo5_defs.h"
```

## Fonctions fournies par `mo5_ctype.h`

```c
int islower(char c);
int isupper(char c);
int isprint(char c);
int ispunct(char c);
```

- `islower(c)` : TRUE si `c` est `'a'`–`'z'`.
- `isupper(c)` : TRUE si `c` est `'A'`–`'Z'`.
- `isprint(c)` : TRUE si `c` est un caractère imprimable (ASCII 32–126), FALSE pour les caractères de contrôle.
- `ispunct(c)` : TRUE pour les ponctuations (`!"#$%...`, `:;<>?@`, `[]^_` etc.).

## Complément à CMOC

CMOC fournit déjà : `isspace`, `isalpha`, `isalnum`, `isdigit`, `tolower`, `toupper`.

`mo5_ctype.h` complète cet ensemble sans redéfinir ce que CMOC gère déjà.

## Exemple

```c
#include "mo5_ctype.h"
#include <cmoc.h>

void analyseChar(char c) {
    if (isupper(c))      fputs("Majuscule");
    else if (islower(c)) fputs("Minuscule");
    else if (isdigit(c)) fputs("Chiffre");
    else if (ispunct(c)) fputs("Ponctuation");
    else if (isprint(c)) fputs("Imprimable");
    else                 fputs("Controle");
}
```

## Notes

- Utilise les fonctions `mo5_ctype` lorsque tu as besoin de tests fins sur le type de caractère, tout en gardant la compatibilité avec CMOC.

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_ctype_h.md`
