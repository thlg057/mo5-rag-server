# Comprendre l'architecture du SDK sprites MO5

## Goal

Présenter la structure générale du SDK graphique MO5 (`mo5_sprite.h/.c`) et expliquer quand utiliser chaque couche.

## Couches d'abstraction

Le SDK est organisé en trois niveaux :

1. **API Actor** (`mo5_actor_*`) – niveau jeu
   - Manipule des entités de jeu (`MO5_Actor`).
   - Gère automatiquement ancienne/nouvelle position.
   - Utilise un rendu optimisé (clear minimal, move fusionné).
2. **API bas niveau** (`mo5_draw/clear/move_sprite`) – niveau graphique avancé
   - Accès direct aux données de sprites (form/couleur).
   - Utile pour HUD, effets spéciaux, tiles de décor.
3. **Matériel** – VRAM / PRC / `row_offsets`
   - Accès direct à la mémoire vidéo et aux registres.
   - Caché derrière les fonctions du SDK.

Représentation (adaptée) :

```
┌─────────────────────────────────────────┐
│           Code de jeu (main.c)          │
├─────────────────────────────────────────┤
│    API Actor  (mo5_actor_*)             │  ← à privilégier
├─────────────────────────────────────────┤
│    API bas niveau (mo5_draw/clear/move) │  ← cas spéciaux
├─────────────────────────────────────────┤
│    VRAM / PRC / row_offsets             │  ← matériel MO5
└─────────────────────────────────────────┘
```

## Fichiers du SDK

| Fichier        | Rôle                                              |
|----------------|---------------------------------------------------|
| `mo5_sprite.h` | Structures, constantes, déclarations              |
| `mo5_sprite.c` | Implémentation de toutes les fonctions graphiques |
| `png2mo5.py`   | Conversion PNG → données sprite pour le MO5       |

## Quand utiliser chaque couche

- **API Actor** :
  - Joueur, ennemis, projectiles, NPC.
  - Tout ce qui se déplace régulièrement et doit être optimisé.
- **API bas niveau** :
  - HUD, barres de vie, texte custom.
  - Effets ponctuels (explosions, décor non acteur).
- **Accès matériel direct** :
  - Réservé aux fonctions internes du SDK.
  - À éviter dans le code de jeu pour garder la portabilité du code.

Source: `mo5-docs/mo5/mo5_sprite.md`

