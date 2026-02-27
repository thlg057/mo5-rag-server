# Utiliser les fonctions CMOC les plus utiles pour le MO5

Ce chunk résume les fonctions de `<cmoc.h>` particulièrement utiles pour un projet Thomson MO5.

## Goal

- Identifier les fonctions CMOC “incontournables” pour un jeu ou programme MO5.
- Avoir une checklist rapide pour structurer ton code.

## Mémoire (VRAM, buffers, structures)

- `memset()` : remplir un bloc mémoire avec un octet.
- `memset16()` : remplir avec un mot 16 bits (pratique pour VRAM alignée).
- `memcpy()` / `memmove()` : copier des blocs (sprites, buffers, tables…).

Usage typique :

```c
// Effacer un buffer de 256 octets
memset(buffer, 0, 256);
```

## Chaînes de caractères

- `sprintf()` : écrire une chaîne formatée dans un buffer (indépendant du terminal).
- `strlen()`, `strcpy()`, `strcat()`, `strcmp()` : manipulations courantes de chaînes.

Tu peux formater d’abord dans un buffer avec `sprintf`, puis afficher avec ton propre code MO5.

## Mathématiques entières

- `divmod16()` / `divmod8()` : division + reste d’un coup, efficace sur MC6809.
- `sqrt16()` / `sqrt32()` : racines carrées entières (distance, physique simple…).
- `abs()` / `labs()` : valeur absolue 16/32 bits.

## Conversions texte ↔ nombres

- `itoa10()` / `utoa10()` : entier → chaîne décimale (score, vies, temps…).
- `atoi()` / `atoui()` : chaîne décimale → entier.

Pattern classique :

```c
char buf[8];
utoa10(score, buf);
// Afficher buf avec ton propre système texte MO5
```

## Tri et recherche génériques

- `qsort()` : tri rapide sur un tableau de structures (classement scores, ennemis…).
- `bsearch()` : recherche dichotomique dans un tableau trié.

## Aléatoire

- `srand()` / `rand()` : générateur pseudo‑aléatoire (`RAND_MAX = 0x7FFF`).

Pense à initialiser le seed une fois au démarrage (valeur fixe pour tests, ou issue d’une saisie utilisateur).

## Bonnes pratiques

- Centralise tes helpers dans un petit module (par ex. `game_math.c`) qui encapsule les appels CMOC.
- Utilise les fonctions “hauts niveaux” (qsort, bsearch, conversions) pour gagner en temps de dev et en lisibilité.

Source: `mo5-docs/cmoc/cmoc.md`, `mo5-docs/cmoc/cmoc_h.md`
