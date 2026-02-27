# Décider quand utiliser la compression RLE sur Thomson MO5

## Goal

Comprendre pourquoi et dans quels cas la compression RLE est utile pour les assets d’un jeu MO5.

## Problème mémoire sur MO5

Sur MO5, la mémoire est très limitée et partagée entre :
- le code du jeu,
- les variables et buffers,
- les graphismes (sprites, niveaux),
- la musique et les effets sonores.

Ordre de grandeur typique (non exhaustif) :
- Sprite 16×16 (forme + couleur) ≈ 64 octets.
- 50 sprites ≈ 3,2 Ko.
- 1 niveau (tilemap 40×25) ≈ 1 Ko.
- 10 niveaux ≈ 10 Ko.
- Musique + effets ≈ 1–5 Ko.

Résultat : les assets peuvent facilement consommer **15–20 Ko** sur une machine qui n’a que 48 Ko de RAM, dont une partie est réservée au système.

La compression permet de stocker **plus de contenu** (sprites, niveaux, musique) dans la même quantité de mémoire ou de ROM.

## Quand le RLE fonctionne bien

Le RLE (Run-Length Encoding) est très efficace sur les données contenant de **longues séquences répétées** :

- **Ciel / mer / grands aplats** :
  - Beaucoup de zéros ou de mêmes valeurs.
  - Exemple classique : ligne de ciel entièrement vide.
- **Tilemaps de niveaux** :
  - Tuiles répétées sur de longues séquences (sols, murs, fonds).
- **Sprites avec fond transparent** :
  - Les pixels de fond sont souvent 0x00 → longues suites de 0.
- **Musique / séquences d’événements** :
  - Notes ou événements qui se répètent.

Dans ces cas, le RLE peut réduire la taille de 30 % à 70 % (voire plus sur de gros aplats).

## Quand éviter le RLE

Le RLE peut être **contre-productif** sur :

- Des motifs très alternés (ex : 0xFF, 0x00, 0xFF, 0x00, …).
- Des données très détaillées (sprites très texturés, bruit).
- Des données quasi aléatoires.

Dans ces situations :
- Le RLE simple `(count, value)` peut **doubler** la taille (2 octets pour stocker 1 valeur).
- Même un RLE amélioré peut n’apporter aucun gain.

**Règle pratique** :
- Utiliser le RLE pour : décors, fonds, tilemaps, données à grands aplats.
- Tester sur quelques sprites ou niveaux avant de généraliser.

## Quand décompresser les données

La décompression RLE a un **coût CPU** :

- Ne **jamais** décompresser un sprite à chaque frame.
- Préférer :
  - Décompresser un niveau **une fois** au chargement dans un buffer RAM.
  - Décompresser un sprite **au moment du spawn** dans un buffer dédié.
  - Garder en RAM décompressée les sprites fréquemment utilisés (joueur, ennemis les plus communs).

Résumé :
- **ROM/cartouche** : données compressées.
- **RAM** : versions décompressées prêtes à l’emploi pour l’affichage.

Source: `mo5-docs/mo5/compression-rle-MO5.md`

