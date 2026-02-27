# Utiliser les constantes et macros utilitaires de `<cmoc.h>`

Ce chunk recense quelques constantes et macros fournies par `<cmoc.h>`.

## Goal

- Connaître les constantes et macros utiles pour écrire du C portable avec CMOC.

## Constantes principales

```c
#define NULL     ((void *) 0)
#define SIZE_MAX  0xFFFFu
#define SSIZE_MAX 0x7FFF
#define RAND_MAX  0x7FFF
```

- `NULL` : pointeur nul standard.
- `SIZE_MAX` : valeur maximale de `size_t` (16 bits non signé).
- `SSIZE_MAX` : valeur max de `ssize_t` (16 bits signé).
- `RAND_MAX` : borne supérieure pour `rand()`.

## Macro `offsetof`

```c
#define offsetof(Type, member)  ((unsigned) &((Type *) 0)->member)
```

- Calcule l’offset, en octets, du champ `member` à l’intérieur de la structure `Type`.
- Très utile pour des opérations bas niveau (sérialisation, buffers, etc.).

Exemple :

```c
typedef struct {
    char id;
    int  score;
} Player;

unsigned offScore = offsetof(Player, score);  // typiquement 1 sur CMOC
```

## Notes

- Ces définitions sont indépendantes de la plateforme (CoCo, MO5, etc.), tant que l’on reste dans le modèle 16 bits de CMOC.

Source: `mo5-docs/cmoc/cmoc_h.md`
