# Understand MO5 Video Memory Layout (Thomson MO5)

Le MO5 utilise une mémoire vidéo de 8 Ko partagée entre CPU et hardware vidéo, organisée en deux banques sélectionnées par bank-switch.

Source : Manuel Technique MO5 p.8-12 — Fiabilité ÉLEVÉE.

## Spécifications

- Résolution : 320×200 pixels
- 1 octet = 8 pixels horizontaux → 40 octets par ligne
- 200 lignes → 8 000 octets par banque
- Base VRAM : `$0000`

## Deux banques (bank-switch via $A7C0 bit 0)

| Banque | Sélection | Rôle | Contenu |
|--------|-----------|------|---------|
| COULEUR | bit 0 = 0 | Attributs couleur | 1 octet par groupe de 8px : `FFFFBBBB` |
| FORME | bit 0 = 1 | Bitmap | 1 bit par pixel : 1=forme, 0=fond |

```c
#define PRC      ((unsigned char *)0xA7C0)
#define VRAM     ((unsigned char *)0x0000)

/* Sélectionner banque COULEUR */
*PRC &= ~0x01;
/* Sélectionner banque FORME */
*PRC |=  0x01;
```

## Calcul d'offset VRAM

```c
/* Offset d'un octet à position (tx, ty) */
/* tx = colonne en octets (0-39), ty = ligne en pixels (0-199) */
unsigned int offset = row_offsets[ty] + tx;

/* row_offsets est précalculé par mo5_video_init() */
/* row_offsets[y] = y * 40 */
```

> ⚠️ Ne pas calculer `y * 40` à l'exécution — le 6809 n'a pas de multiplication hardware.
> Utiliser la table `row_offsets[]` précalculée.

## Format octet couleur (banque COULEUR)

```
bit 7–4 : index couleur FORME (fg) — 0 à 15
bit 3–0 : index couleur FOND  (bg) — 0 à 15
```

## Contrainte hardware — color clash

Chaque octet de la banque COULEUR couvre **8 pixels horizontaux**. Ce groupe partage obligatoirement 2 couleurs (fond + forme). Il est **impossible** d'avoir plus de 2 couleurs différentes dans un même groupe de 8 pixels.

## Séquence d'écriture d'un sprite

```c
/* 1. Écrire la couleur */
*PRC &= ~0x01;                  /* banque COULEUR */
for (i = 0; i < h; i++)
    for (j = 0; j < w; j++)
        VRAM[row_offsets[ty+i] + tx+j] = color_data[...];

/* 2. Écrire la forme */
*PRC |= 0x01;                   /* banque FORME */
for (i = 0; i < h; i++)
    for (j = 0; j < w; j++)
        VRAM[row_offsets[ty+i] + tx+j] = form_data[...];
```

## Erreurs fréquentes

```text
❌ Oublier de sélectionner la banque avant écriture → données dans la mauvaise banque
❌ Calculer y * 40 à l'exécution → lent, utiliser row_offsets[]
❌ Confondre tx (en octets) et x (en pixels) → tx = x / 8
❌ Supposer que X est en pixels → X est en octets (0-39)
❌ Plus de 2 couleurs dans 8px → color clash hardware
```

Source: `mo5_hardware_reference.md` sections 3 et 5
