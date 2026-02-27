# Choisir outils et émulateur pour le développement MO5

## Goal

Lister les outils recommandés pour développer et tester un jeu sur Thomson MO5.

## Assembleur et toolchain

Le guide recommande :

- **Assembleur 6809** : `lwasm` (suite **lwtools**) – très adapté au cross-dev.
- **Linker** : `lwlink` (et `lwar` si création de librairies).
- **Cross-compilateur C** : CMOC si l’on veut écrire une partie de la logique de haut niveau en C.

Stratégie :
- Écrire les parties critiques (affichage, boucle de jeu) en **assembleur 6809**.
- Garder le C pour la logique de plus haut niveau quand les performances le permettent.

## Émulateurs recommandés

- **MAME** avec le profil `mo5`.
- **Theodora** (autre émulateur dédié Thomson).

L’émulateur permet :
- un cycle rapide « compiler → lancer → profiler → ajuster »,
- de tester facilement sur plusieurs configurations,
- de visualiser le comportement vidéo (flicker, tearing, etc.).

## Workflow conseillé

1. Développer et compiler sur PC (assembleur/C via lwtools/CMOC).
2. Générer un binaire `.bin` pour le MO5.
3. Charger ce binaire dans l’émulateur (MAME, Theodora).
4. Profiler et ajuster :
   - mesurer la charge CPU (20 000 cycles par frame à 50 Hz),
   - vérifier la stabilité de la boucle de jeu,
   - affiner les timings d’affichage et de son.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

