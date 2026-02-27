# Utiliser les fonctions de conversion texte ↔ nombres de CMOC

Ce chunk regroupe les fonctions de `<cmoc.h>` qui convertissent des nombres en chaînes et inversement.

## Goal

- Afficher des nombres (score, vies, temps…) en texte.
- Parser des nombres saisis ou stockés sous forme de chaînes.

## Chaîne → nombre

- `unsigned atoui(const char *s)`
- `int atoi(const char *s)`
- `long atol(const char *s)`
- `unsigned long atoul(const char *s)`
- `unsigned long atoul16(const char *s)` (entrée hexadécimale)
- `long atol16(const char *s)` (entrée hexadécimale signée)
- `unsigned long strtoul10(const char *nptr, char **endptr)`
- `unsigned long strtoul16(const char *nptr, char **endptr)`

Exemple :

```c
int value = atoi("1234");
unsigned long addr = atoul16("A000");
```

## Nombre → chaîne

- `char *itoa10(int value, char *str)`
- `char *utoa10(unsigned value, char *str)`
- `char *ltoa10(long value, char *str)`
- `char *ultoa10(unsigned long value, char *str)`
- `char *ultoa16(unsigned long value, char *str)`
- `char *dwtoa(char *out, unsigned hi, unsigned lo)` (double‑mot hi:lo → décimal)

Exemple typique pour afficher un score :

```c
char scoreText[8];
utoa10(score, scoreText);
// Afficher scoreText avec ta routine texte MO5
```

## Notes

- Toutes ces fonctions sont **purement logicielles** et ne dépendent pas du matériel (CoCo ou MO5).
- Assure‑toi que les buffers de sortie sont assez grands (au moins 6–7 caractères pour des valeurs 16 bits avec signe).

Source: `mo5-docs/cmoc/cmoc_h.md`
