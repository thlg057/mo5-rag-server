# Utiliser `sbrk` et `sbrkmax` CMOC en sécurité sur MO5

Ce chunk explique le fonctionnement de `sbrk`/`sbrkmax` et les précautions à prendre sur MO5.

## Goal

- Comprendre ce que fait `sbrk` (allocation de tas simple).
- Savoir quand **ne pas** l’utiliser dans un contexte MO5.

## Rappel : API

```c
void *sbrk(size_t increment);
size_t sbrkmax(void);
```

- `sbrk(increment)` avance le pointeur de fin de tas de `increment` octets.
  - Retourne l’ancienne valeur du pointeur (début du bloc alloué) en cas de succès.
  - Retourne `(void *) -1` si la mémoire n’est pas disponible.
- `sbrkmax()` retourne le nombre d’octets encore allouables.

## Exemple générique

```c
void *p = sbrk(100);
if (p != (void *) -1) {
    memset(p, 'X', 100);
}
```

## Problème de placement mémoire

Dans la doc CMOC, `sbrk` est pensé pour CoCo/Dragon, avec une mise en mémoire bien précise (programme chargé après le BASIC et ses variables, tas qui monte vers la pile système, etc.).

Sur MO5, la disposition mémoire réelle dépend :

- de la façon dont tu charges le binaire (moniteur, BASIC Thomson, cartouche…),
- de la présence éventuelle d’une VRAM mappée bas, de ROMs, etc.

**Si ces hypothèses ne sont pas respectées, `sbrk` peut empiéter sur la pile, les buffers système ou la VRAM.**

## Recommandations pour MO5

- Utilise `sbrk` **uniquement** si tu maîtrises exactement la carte mémoire (layout précis, tests sur machine ou émulateur).
- Préfère un **layout mémoire statique** (tableaux globaux, buffers fixes) tant que possible.
- Si tu l’utilises, commence par appeler `sbrkmax()` pour vérifier qu’il reste une marge suffisante.

Source: `mo5-docs/cmoc/cmoc_h.md`, `mo5-docs/cmoc/cmoc-manual.md`
