# Éviter les fonctions CMOC dépendantes du terminal ou de la virgule flottante sur MO5

Ce chunk liste les fonctions de `<cmoc.h>` qui supposent un environnement CoCo spécifique ou la présence de routines flottantes non pertinentes sur MO5.

## Goal

- Savoir quelles fonctions **ne pas utiliser** directement sur MO5.
- Prévoir des alternatives adaptées au Thomson.

## Fonctions dépendantes du terminal CoCo

Dans `<cmoc.h>` :

- `void delay(size_t sixtiethsOfASecond);`
- `unsigned readword(void);`
- `char *readline(void);`

Problèmes :

- Ces fonctions reposent sur le terminal/console CoCo (routines ROM, fréquence 60 Hz, etc.).
- Sur MO5, elles n’ont pas de support direct.

Alternatives MO5 :

- `delay` : écrire une boucle de délai calibrée sur la fréquence MO5 (ou utiliser VBL si tu as un hook adapté).
- `readword` / `readline` : implémenter la lecture clavier et l’édition de ligne via tes propres routines Thomson ou via un SDK MO5.

## Fonctions de virgule flottante

La doc mentionne que des fonctions comme :

- `strtof`, `atoff`, `ftoa`, `logf`, `log2f`, etc.

dépendent d’options de compilation particulières (`--mc6839`, `_COCO_BASIC_`, `DRAGON`, `_CMOC_NATIVE_FLOAT_`) et de la présence de routines flottantes côté CoCo/Dragon.

Sur un projet MO5 standard :

- Évite de miser sur ces fonctions.
- Privilégie les entiers et les helpers entiers de CMOC (`sqrt16`, `divmod16`, etc.).

## Recommandation

- Si ton code doit aussi tourner sur CoCo/Dragon, isole ces appels derrière des `#ifdef` ou des wrappers qui choisissent la bonne implémentation selon la plateforme.

Source: `mo5-docs/cmoc/cmoc_h.md`, `mo5-docs/cmoc/cmoc-manual.md`
