# Initialiser le mode graphique avec `mo5_init_graphic_mode` (MO5)

## Goal

Expliquer le rôle de `mo5_init_graphic_mode()` dans le SDK sprite MO5 et quand l’appeler.

## Prototype

```c
void mo5_init_graphic_mode(unsigned char color);
```

## Effets de cette fonction

`mo5_init_graphic_mode()` doit être appelée **une seule fois** au démarrage du programme, avant tout dessin :

- Calcule la table `row_offsets` (offset mémoire de chaque ligne écran) pour éviter les multiplications à l’affichage.
- Configure le MO5 en **mode graphique**.
- Remplit l’écran avec la couleur donnée.

Exemple d’appel :

```c
// Fond noir (forme et fond noirs)
mo5_init_graphic_mode(COLOR(C_BLACK, C_BLACK));
```

## Bonnes pratiques

- Appeler cette fonction **avant** d’initialiser ou de dessiner des sprites.
- Conserver la palette et les constantes écran fournies par le SDK (`SCREEN_WIDTH_BYTES`, etc.) pour tous les calculs de position.
- Prévoir la couleur de fond choisie dès la conception des assets (fonds transparents, contraste des sprites).

Source: `mo5-docs/mo5/mo5_sprite.md`

