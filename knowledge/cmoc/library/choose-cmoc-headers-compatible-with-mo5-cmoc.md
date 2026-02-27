# Choisir les bons headers CMOC compatibles avec le MO5

Ce chunk résume les headers CMOC documentés pour le MO5 et leur statut.

## Goal

- Savoir quels headers CMOC inclure dans un projet MO5.
- Éviter d’inclure des headers internes ou spécifiques CoCo/Dragon.

## Headers recommandés sur MO5

- `<cmoc.h>` : bibliothèque standard CMOC (chaînes, mémoire, maths, conversions, tri, aléatoire...).
- `<stdarg.h>` : support des fonctions variadiques (`va_list`, `va_start`, `va_arg`, `va_end`). Inclus automatiquement par `cmoc.h`, mais peut être inclus explicitement.
- `<setjmp.h>` : mécanisme de saut non local (`setjmp` / `longjmp`).
- `<assert.h>` : assertions de debug configurables.

Ces headers sont **totalement utilisables** sur MO5, sous réserve de respecter leurs limitations spécifiques documentées dans les autres chunks.

## Headers à ne pas inclure directement

- `<assert-impl.h>` : implémentation interne de `<assert.h>`. **Ne pas** inclure directement.
- `<cmoc-stdlib-private.h>` : détails internes du runtime CMOC.

Ces fichiers sont gérés automatiquement par CMOC ; ton code applicatif ne doit pas les référencer.

## Headers non compatibles MO5

- `<disk.h>` : accès disque CoCo Disk Basic (`$FF40`+). Non utilisable tel quel sur Thomson.
- `<dskcon-standalone.h>` : contrôleur WD1793 côté CoCo/Dragon. Non utilisable tel quel sur Thomson.
- `<coco.h>` : API spécifique au hardware CoCo/Dragon.

Pour la partie “disque” sur Thomson, il faut prévoir une implémentation spécifique (autre bibliothèque ou code maison).

## Header d’inspiration pour les types de base

- `<vectrex.h>` : non destiné à être inclus dans un projet MO5, mais sert de **modèle** pour définir des types pratiques (`byte`, `word`, `uint32_t`, etc.) dans ton propre header `mo5.h`.

Exemple minimal de `mo5.h` inspiré de `vectrex.h` :

```c
// mo5.h
typedef unsigned char  byte;
typedef signed char    sbyte;
typedef unsigned int   word;
typedef signed int     sword;
typedef unsigned long  uint32_t;
typedef signed long    int32_t;
enum { FALSE = 0, TRUE = 1 };
```

## Récapitulatif

- ✅ À utiliser : `<cmoc.h>`, `<stdarg.h>`, `<setjmp.h>`, `<assert.h>`.
- ⚠️ Internes : `<assert-impl.h>`, `<cmoc-stdlib-private.h>` (ne pas inclure).
- ❌ Spécifiques CoCo/Dragon : `<disk.h>`, `<dskcon-standalone.h>`, `<coco.h>`.

Source: `mo5-docs/cmoc/cmoc.md`
