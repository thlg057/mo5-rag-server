# Utiliser les fonctions mémoire CMOC pour VRAM et buffers

Ce chunk présente les fonctions mémoire de `<cmoc.h>` et leurs usages typiques, notamment pour la VRAM MO5.

## Goal

- Savoir choisir entre `memcpy`, `memmove`, `memset`, `memset16`.
- Manipuler efficacement buffers et zones mémoire.

## Fonctions disponibles

- Comparaison : `memcmp`, `memicmp`
- Recherche : `memchr`, `memichr`
- Copie : `memcpy`, `memmove`
- Remplissage : `memset`, `memset16`

Toutes ces fonctions sont **totalement portables** pour le code MO5.

## Copier un bloc mémoire

```c
// Copier n octets de src vers dest
memcpy(dest, src, n);
```

Utilise `memmove` si les zones peuvent se chevaucher :

```c
memmove(dest, src, n);
```

## Remplir une zone

```c
// Mettre toute une zone à 0xFF
memset(buffer, 0xFF, size);
```

Pour remplir par mots 16 bits (pattern répété tous les 2 octets) :

```c
unsigned short pattern = 0xF0F0;
memset16(vram, pattern, numWords);
```

`memset16` est particulièrement utile pour des zones VRAM alignées (par exemple palette ou structures répétitives).

## Recherche dans un buffer

```c
void *p = memchr(buffer, 0, size);
if (p != 0) {
    // trouvé un octet nul
}
```

`memichr`/`memicmp` permettent des comparaisons insensibles à la casse sur des données ASCII.

## Notes

- Pense systématiquement à la taille réelle en octets, surtout si tu manipules des structures : `sizeof(struct)` plutôt que une constante magique.
- Sur MO5, ces primitives bas niveau sont souvent plus simples et plus sûres que d’écrire des boucles assembleur à la main.

Source: `mo5-docs/cmoc/cmoc_h.md`
