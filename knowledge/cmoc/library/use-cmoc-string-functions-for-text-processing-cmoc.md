# Utiliser les fonctions de chaînes CMOC pour le traitement de texte

Ce chunk regroupe les fonctions de manipulation de chaînes de `<cmoc.h>`.

## Goal

- Savoir quelles fonctions de chaînes CMOC sont disponibles.
- Les utiliser pour parser et transformer du texte sur MO5.

## Fonctions principales

Toutes ces fonctions sont **totalement portables** côté MO5 :

- Comparaison : `strcmp`, `stricmp`, `strncmp`
- Longueur : `strlen`
- Copie : `strcpy`, `strncpy`
- Concaténation : `strcat`
- Recherche de caractères : `strchr`, `strrchr`, `strpbrk`
- Recherche de sous‑chaîne : `strstr`
- Transformations : `strlwr`, `strupr`
- Préfixes : `strspn`, `strcspn`
- Tokenisation : `strtok` (non réentrant)

## Exemples rapides

Comparer deux chaînes sans tenir compte de la casse :

```c
if (stricmp(cmd, "start") == 0) {
    // lancer le jeu
}
```

Découper une chaîne en tokens séparés par des espaces :

```c
char *token = strtok(line, " ");
while (token != 0) {
    // traiter token
    token = strtok(0, " ");
}
```

Convertir une chaîne en majuscules :

```c
strupr(buffer);
```

## Notes

- `strtok` utilise un état interne global : évite de l’utiliser en parallèle dans plusieurs fonctions sans précaution.
- Pour du parsing plus robuste, tu peux combiner `strspn`/`strcspn` avec des boucles manuelles.

Source: `mo5-docs/cmoc/cmoc_h.md`
