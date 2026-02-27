# Anticiper les contraintes matérielles du MO5 dès la conception du jeu

## Goal

Synthétiser les principales contraintes du MO5 et leurs implications pour le game design et l’architecture du code.

## Principales contraintes

| Contrainte               | Implication principale                           |
|--------------------------|--------------------------------------------------|
| ~20 000 cycles/frame     | Profiler tôt, optimiser les parties critiques    |
| Clash couleur (8px/2col) | Concevoir graphismes en tenant compte de 2 col/8px |
| Pas de sprites hardware  | Limiter le nombre de sprites, tout en logiciel  |
| Pas de multi-process     | Architecture en machine à états obligatoire      |
| Mémoire statique         | Layout mémoire figé, pas de `malloc`            |
| Son bloquant par défaut  | Prévoir un player VBL pour musique de fond      |
| Joystick analogique lent | Ne pas le lire à chaque frame                   |
| Sauvegarde difficile     | Privilégier cartouche RAM ou éviter la sauvegarde|

## Conséquences pratiques

- **Design graphique** :
  - penser en blocs de 8 pixels avec 2 couleurs.
  - éviter les HUD trop détaillés en couleurs variées.
- **Architecture du moteur** :
  - structurer en machine à états (menu, jeu, chargement…).
  - découper les opérations longues sur plusieurs frames.
- **Gestion des ressources** :
  - planifier un layout mémoire statique,
  - compresser les assets (RLE) et décompresser au chargement.
- **Audio** :
  - décider tôt si le jeu a besoin de musique continue,
  - concevoir un player non bloquant si oui.

## Recommandation générale

Avant d’écrire la première ligne de code :
- lister les besoins en sprites, niveaux, audio,
- estimer leur poids mémoire et leur coût CPU,
- valider qu’ils tiennent dans :
  - 48 Ko de RAM,
  - ~20 000 cycles par frame.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

