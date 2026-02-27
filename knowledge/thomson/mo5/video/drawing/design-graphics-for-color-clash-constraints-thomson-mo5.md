# Concevoir des graphismes avec les contraintes couleur du MO5

## Goal

Expliquer le modèle graphique du MO5 et comment concevoir des sprites/décors compatibles avec les contraintes de couleur.

## Modèle graphique du MO5

- Résolution : **320×200 pixels**.
- Zone **forme** (bitmap) : chaque octet encode 8 pixels horizontaux (1 bit/pixel).
- Zone **couleur** : pour chaque octet de forme, un octet couleur associé décrit **2 couleurs possibles** pour ces 8 pixels.

Conséquence :
- Sur un bloc horizontal de 8 pixels, on ne peut afficher que **2 couleurs** (fond + forme).
- Le comportement est similaire au « clash couleur » du ZX Spectrum.

## Implications pour les graphistes

- Impossible d’avoir 3+ couleurs différentes dans un même groupe de 8 pixels horizontaux.
- Les sprites doivent être conçus **en bandes de 8 pixels** avec au plus 2 couleurs par bande.
- Les décors (tiles, fonds) doivent éviter les motifs qui exigent plus de 2 couleurs dans ces blocs.

## Bonnes pratiques

- Utiliser un outil de dessin qui :
  - travaille en résolution 320×200,
  - simule le **clash couleur** (2 couleurs par groupe de 8px).
- Concevoir dès le départ :
  - la palette globale du jeu,
  - les sprites et tiles avec des combinaisons de couleurs compatibles.
- Tester régulièrement les graphismes sur émulateur pour détecter :
  - des zones où le clash est trop agressif,
  - des textes illisibles à cause du contraste.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

