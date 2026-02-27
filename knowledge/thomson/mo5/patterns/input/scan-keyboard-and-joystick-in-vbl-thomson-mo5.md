# Scanner clavier et joystick dans la VBL sur Thomson MO5

## Goal

Décrire une stratégie robuste pour lire clavier et joystick sur MO5 en utilisant la VBL.

## Clavier : matrice scannée via PIA 6821

- Le clavier est organisé en **matrice** (lignes/colonnes) lue via la PIA 6821.
- Bonne pratique :
  - scanner la matrice dans l’**interruption VBL** ou dans un handler dédié,
  - stocker l’état dans des variables globales,
  - lire ces variables depuis la boucle principale.

Avantages :
- Lecture cohérente entre deux frames (moins de rebond).
- Possibilité de détecter les **transitions** (touche venant d’être pressée) :
  - comparer état courant et état précédent.
- Gestion facile de l’**autorepeat** avec un simple compteur par touche.

## Joystick : entrées analogiques lentes

- Les joysticks Thomson sont analogiques, lus via un convertisseur A/N interne.
- La lecture complète peut consommer beaucoup de cycles CPU.

Recommandations :
- Lire le joystick dans la VBL, **pas** obligatoirement à chaque frame.
- Une lecture toutes les 2–3 frames est généralement suffisante pour un jeu d’action.

## Pattern général

1. Dans la VBL :
   - scanner clavier + joystick,
   - mettre à jour `keys_current`, `keys_previous`, `joy_state`.
2. Dans la boucle de jeu :
   - utiliser ces états pré-calculés pour la logique (mouvements, tirs, menus).

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

