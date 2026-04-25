# Implement Game Loop with VBL Synchronization (Thomson MO5)

Deux stratégies pour synchroniser la boucle de jeu sur le VBL à 50 Hz.

## Stratégie A — VBL en début de boucle (recommandée pour débuter)

```c
while (1) {
    mo5_wait_vbl();       /* synchronisation trame */
    read_inputs();
    update_logic();
    draw_all();
}
```

Logique + dessin dans les 20ms de la trame complète.
Simple à implémenter, facile à raisonner.

## Stratégie B — VBL juste avant le dessin (avancée)

```c
while (1) {
    read_inputs();
    update_logic();       /* pendant le balayage actif (~18.7ms) */
    mo5_wait_vbl();
    draw_all();           /* pendant la fenêtre VBL (~1.2ms) */
}
```

Maximise le temps CPU pour la logique. Si `draw_all()` dépasse ~1 200 cycles,
des artefacts peuvent apparaître en bas d'écran.

## Comparaison

| | Stratégie A | Stratégie B |
|---|---|---|
| Simplicité | ✓ Simple | Plus structuré |
| Budget logique | ~20ms | ~18.7ms dédiés |
| Budget dessin | ~20ms | ~1.2ms dédiés |
| Risque tearing | Faible | Minimal |
| Usage recommandé | Jeux simples | Beaucoup d'entités |

## Notes

- Le MO5 fonctionne en PAL à 50 Hz — 20ms par frame, ~20 000 cycles
- `mo5_wait_vbl()` se positionne sur le front montant de INITRAME
- La synchronisation est propre quelle que soit la durée de la frame précédente

Source: `mo5_hardware_reference.md` section 4
