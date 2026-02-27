# Comprendre la VBL (Vertical Blank) sur Thomson MO5

## Goal

Expliquer ce qu'est la VBL, comment le balayage de l'écran fonctionne sur MO5, et pourquoi la synchronisation sur la VBL est indispensable pour un jeu stable.

## Concept

- L'écran est redessiné **50 fois par seconde** (50 Hz PAL).
- Une frame est découpée en deux phases :
  - **Balayage actif** (~18.7 ms) : le faisceau balaie les lignes visibles.
  - **Retour vertical / VBL** (~1.2 ms) : le faisceau remonte en haut, rien n'est affiché.

Schéma simplifié :

```
   balayage actif        VBL
|████████████████████████████|____|████████████████████████████|____|
0ms                        18.7ms 20ms                        38.7ms 40ms
```

## Pourquoi c'est important pour un jeu

### Vitesse de jeu stable

Sans VBL :
- La boucle de jeu tourne "à fond" selon la charge CPU.
- Ajouter des ennemis ou des calculs **ralentit** le jeu.
- En retirer **accélère** le jeu.
- Le gameplay devient dépendant du contenu de la frame.

Avec VBL :
- On exécute **exactement 50 itérations de boucle par seconde**.
- Le timing du jeu devient prévisible et indépendant des légères variations de charge.

### Éviter le flickering / tearing

Modifier la VRAM pendant le balayage actif peut donner :
- Une partie de l'ancien sprite + une partie du nouveau sur la même frame.
- Des "déchirures" visuelles quand le faisceau lit une ligne en cours de modification.

En concentrant les écritures critiques dans la VBL :
- On met à jour l'image **entre deux balayages complets**.
- L'écran affiche toujours un état cohérent (avant ou après mise à jour, mais jamais un mélange).

## Règle de conception

- Toujours concevoir la boucle de jeu autour de la VBL :
  - Logique de jeu pendant le balayage actif.
  - Dessin principalement pendant la VBL (ou juste après l'attente VBL).

Source: `mo5-docs/mo5/vbl_mo5.md`

