# Utiliser `mo5_clamp` pour borner des valeurs avec le SDK MO5 (Thomson MO5)

Ce chunk décrit la fonction utilitaire `mo5_clamp` fournie par `mo5_utils.h`.

## Goal

- Contraindre une valeur entière dans un intervalle `[min, max]`.
- Réutiliser ce pattern partout (vitesse, coordonnées, indices...).

## Prototype

```c
int mo5_clamp(int value, int min, int max);
```

- Retourne `min` si `value < min`.
- Retourne `max` si `value > max`.
- Retourne `value` sinon.
- Comportement indéfini si `min > max`.

## Exemples d'utilisation

Limiter la vitesse d'un sprite :

```c
speed = mo5_clamp(speed, -3, 3);
```

Limiter une coordonnée X aux bords de l'écran (axe X en octets) :

```c
int tx = mo5_clamp(new_tx, 0, SCREEN_WIDTH_BYTES - sprite_w);
```

## Relation avec `mo5_actor_clamp`

- `mo5_clamp` agit sur un entier isolé.
- `mo5_actor_clamp` (dans `mo5_sprite.h`) clamp la position d'un `MO5_Actor` complet en tenant compte de la taille du sprite.

Utilise `mo5_clamp` pour les calculs manuels (vitesse, indices, positions calculées avant d'appeler une API plus haut niveau).

## Notes

Note : ces fonctions font partie du SDK MO5 (sdk_mo5 : https://github.com/thlg057/sdk_mo5). Tu dois intégrer ce SDK à ton projet pour les utiliser.

Source: `mo5-docs/mo5_sdk/mo5_utils_h.md`
