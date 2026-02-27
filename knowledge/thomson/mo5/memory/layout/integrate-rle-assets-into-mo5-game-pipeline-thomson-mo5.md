# Intégrer des assets RLE dans le pipeline de jeu Thomson MO5

## Goal

Montrer comment utiliser la compression RLE dans un jeu MO5 : buffers statiques, décompression contrôlée, affichage.

## Buffers statiques (pas de malloc)

Sur MO5, il n’y a pas de `malloc`. On alloue donc des buffers **statiques** pour contenir les données décompressées.

Exemple pour un sprite 16×16 (2 octets par ligne) :

```c
static unsigned char sprite_form_buf[16 * 2];
static unsigned char sprite_color_buf[16 * 2];
```

Exemple pour une tilemap de niveau 40×25 :

```c
static unsigned char level_buffer[40 * 25];
```

## Affichage d’un sprite compressé

Pattern recommandé :

```c
void draw_sprite_compressed(int tx, int ty,
                            const unsigned char *form_rle,
                            const unsigned char *color_rle)
{
    rle_decode_ex(form_rle,  sprite_form_buf);
    rle_decode_ex(color_rle, sprite_color_buf);
    mo5_draw_sprite(tx, ty,
                    sprite_form_buf, sprite_color_buf,
                    2 /* width_bytes */, 16 /* height */);
}
```

Idée :
- Les données compressées résident en ROM ou dans un segment de données compact.
- On décompresse dans un petit buffer RAM juste avant l’affichage.

## Chargement d’un niveau compressé

Pour un niveau entier, on préfère décompresser **une fois** au chargement :

```c
static unsigned char level_buffer[40 * 25];

void load_level(const unsigned char *level_rle)
{
    rle_decode_ex(level_rle, level_buffer);
    /* Le reste du code utilise directement level_buffer */
}
```

Le rendu (dessin de la tilemap) lit ensuite `level_buffer` sans refaire de décompression.

## Pipeline de développement recommandé

Schéma global :

```
Assets bruts (PNG, niveaux texte)
        ↓
  Outil PC (Python)
  → conversion en binaire MO5
  → encodage RLE
  → génération de .h / .bin
        ↓
  Code C MO5
  → include des données compressées
  → décompression dans des buffers RAM statiques
  → affichage depuis ces buffers
```

Avantages :
- Moins de ROM occupée par les données.
- Contrôle précis des zones RAM utilisées pour les buffers décompressés.
- Coût CPU de décompression maîtrisé (uniquement aux moments clefs : chargement, spawn, etc.).

Source: `mo5-docs/mo5/compression-rle-MO5.md`

