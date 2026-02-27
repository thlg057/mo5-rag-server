# Utiliser une machine à états et du travail découpé sur MO5

## Goal

Expliquer pourquoi un jeu MO5 doit être structuré en **machine à états** et comment découper les traitements longs sur plusieurs frames.

## Pas de multi-process sur MO5

- Le MO5 n’a ni threads, ni multitâche.
- Toute opération longue (décompression, chargement, génération) **bloque** la boucle de jeu si elle est faite d’un seul bloc.

## Machine à états

Approche recommandée :

- Représenter le jeu comme une **machine à états** :
  - états typiques : menu, jeu, pause, mort, chargement, etc.
- Chaque frame :
  - exécuter la logique correspondant à l’état courant,
  - décider éventuellement d’un changement d’état.

```c
enum GameState { STATE_MENU, STATE_PLAY, STATE_LOADING, ... };

void game_update(void) {
    switch (state) {
        case STATE_MENU:   update_menu();    break;
        case STATE_PLAY:   update_play();    break;
        case STATE_LOADING:update_loading(); break;
    }
}
```

## Découper les opérations longues

Pour les tâches coûteuses (décompression RLE, génération de niveau, I/O) :
- ne pas tout faire en une seule frame,
- découper en **petits morceaux** exécutés sur plusieurs frames.

Exemple : chargement par étapes dans `STATE_LOADING` :
- étape 1 : lire les métadonnées,
- étape 2 : décompresser les tiles,
- étape 3 : initialiser les acteurs,
- etc.

Chaque frame, on avance d’une étape jusqu’à ce que le chargement soit terminé, puis on repasse à `STATE_PLAY`.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

