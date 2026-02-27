# Utiliser les "dirty rectangles" pour le rafraîchissement partiel (MO5)

## Goal

Montrer comment remplacer un double buffering complet par une stratégie de rafraîchissement partiel adaptée à la RAM limitée du MO5.

## Problème du double buffering

- Un vrai double buffer vidéo nécessite une **deuxième RAM écran** complète.
- Sur MO5, la mémoire est trop limitée pour conserver deux copies intégrales de l’écran.

## Principe des "dirty rectangles"

Au lieu de redessiner tout l’écran :
- Maintenir une liste des **zones modifiées** (rectangles) à chaque frame.
- Effacer et redessiner **uniquement** ces zones.

Avantages :
- Réduction drastique des écritures VRAM.
- Respect plus facile du budget CPU (~20 000 cycles/frame à 50 Hz).

## Exemple de pattern

Pseudo-code :

```c
// Dans la logique de jeu
dirty_list_clear();
update_entities_and_mark_dirty();

// Avant le dessin
for (chaque dirty_rect dans la liste) {
    clear_rect(dirty_rect);
    draw_rect(dirty_rect);
}
```

Conseils :
- Fusionner les rectangles proches si nécessaire pour limiter leur nombre.
- S’assurer que tous les déplacements d’objets marquent correctement leurs anciennes et nouvelles zones comme "dirty".

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

