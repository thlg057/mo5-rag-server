# Synchroniser la boucle de jeu sur la VBL (Thomson MO5)

## Goal

Montrer comment utiliser `mo5_wait_vbl()` pour cadencer la boucle de jeu à 50 Hz et placer correctement logique et dessin.

## Placement recommandé dans la boucle

La structure idéale :

```c
while (1) {
    // 1. Logique de jeu (pendant le balayage actif)
    game_update_player();
    game_update_bullets();
    game_update_enemies();
    check_collisions();

    // 2. Synchronisation juste avant le dessin
    mo5_wait_vbl();

    // 3. Dessin (pendant la VBL)
    draw_all();
}
```

Explications :
- La **logique** s'exécute pendant les ~18.7 ms de balayage actif (temps "gratuit").
- `mo5_wait_vbl()` bloque jusqu'au début de la prochaine VBL.
- Le **dessin** se fait dans ou juste après la VBL (~1.2 ms), limitant les risques de tearing.

## Variante à éviter

```c
while (1) {
    mo5_wait_vbl();      // Attente en début de boucle → temps gaspillé
    draw_all();
    game_update_player();
    game_update_enemies();
}
```

Problèmes :
- On passe la VBL à attendre sans dessiner.
- La logique de jeu se retrouve comprimée avec le dessin dans la fenêtre courte de VBL.
- On perd la grande plage de temps du balayage actif.

## Budget temps approximatif

- Fréquence : 50 Hz → **20 ms par frame**.
- VBL : ~1.2 ms → **≈ 1200 cycles** à 1 MHz.
- Ordres de grandeur typiques (16×16) :
  - `clear_sprite` ≈ 200 cycles.
  - `draw_sprite`  ≈ 200 cycles.
- Pour beaucoup de sprites, préférer une fonction optimisée type `move_sprite` qui fusionne clear + draw.

Source: `mo5-docs/mo5/vbl_mo5.md`

