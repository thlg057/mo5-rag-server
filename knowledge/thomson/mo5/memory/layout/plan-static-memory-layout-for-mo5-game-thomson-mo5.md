# Planifier un layout mémoire statique pour un jeu Thomson MO5

## Goal

Expliquer pourquoi il n’y a pas de `malloc` sur MO5 et comment planifier un layout mémoire statique pour un jeu.

## Pas de `malloc` sur MO5

Le MO5 dispose de 48 Ko de RAM, mais :
- la RAM écran et les variables système occupent une partie fixe de la mémoire,
- le BASIC/moniteur et les I/O sont mappés en ROM/I/O,
- la RAM « utilisateur » est limitée.

Conséquence :
- **aucune allocation dynamique** (pas de `malloc`),
- tout doit être alloué **statiquement** (tables, sprites, buffers, niveaux).

## Idée générale du mapping

Schéma simplifié :

| Zone               | Usage typique                              |
|--------------------|--------------------------------------------|
| RAM écran (forme)  | Bitmap vidéo (pixels)                      |
| Variables système  | Géré par le système/moniteur               |
| RAM utilisateur    | Code du jeu, données, buffers              |
| ROM/Cartouche      | Code final, tables, graphismes compressés  |

Le guide insiste : il faut **planifier dès le début** où placer :
- le code (bank ROM ou RAM utilisateur),
- les données globales (tables, états de jeu),
- les buffers temporaires (décompression, audio, etc.),
- les données graphiques (formes/couleurs de sprites, tilemaps).

## Stratégie de layout statique

1. Faire l’inventaire de tout ce qui doit résider en RAM :
   - buffers graphiques temporaires (sprites décompressés, tilemap active),
   - états de jeu (joueurs, ennemis, bullets),
   - buffers son éventuels,
   - pile C/assembleur.
2. Estimer la taille de chaque bloc et réserver des zones non chevauchantes.
3. Garder une **marge de sécurité** pour la pile et les variables temporaires.
4. Mettre tout ce qui est possible en ROM/cartouche (données compressées, tables constantes).

## Règle pratique

- Éviter les grandes structures globales dupliquées :
  - partager les données de sprite entre plusieurs acteurs,
  - réutiliser les buffers (ex: buffer de décompression commun à plusieurs assets).
- Documenter le layout final (tableau d’adresses) pour éviter les recouvrements.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

