# Utiliser des fonctions C de décodage RLE sur Thomson MO5

## Goal

Fournir des fonctions C réutilisables pour décompresser des données RLE sur MO5.

## Décodeur RLE simple `(count, value)`

```c
/* RLE simple (count, value) avec token de fin count=0 */
void rle_decode(const unsigned char *src,
                unsigned char *dst,
                int dst_size)
{
    int out = 0;
    while (out < dst_size) {
        unsigned char count = *src++;
        unsigned char value = *src++;
        if (count == 0)
            break;  // fin de stream
        while (count-- && out < dst_size)
            dst[out++] = value;
    }
}
```

Caractéristiques :
- `src` pointe vers les données RLE (en ROM ou en RAM).
- `dst` pointe vers un buffer RAM suffisamment grand.
- `dst_size` protège contre un dépassement en cas de données corrompues.
- Un `count` nul (`0x00`) sert de **fin de flux**.

## Décodeur RLE avec mode littéral

```c
/* RLE avec mode littéral (bit7 = mode, 0 = répétition, 1 = littéral) */
void rle_decode_ex(const unsigned char *src,
                   unsigned char *dst)
{
    unsigned char header;
    int count, i;
    unsigned char *out = dst;

    while ((header = *src++) != 0x00) {
        if (header & 0x80) {
            /* Mode littéral : copier count octets tels quels */
            count = header & 0x7F;
            for (i = 0; i < count; i++)
                *out++ = *src++;
        } else {
            /* Mode répétition : répéter la valeur count fois */
            count = header;
            unsigned char value = *src++;
            for (i = 0; i < count; i++)
                *out++ = value;
        }
    }
}
```

Caractéristiques :
- Le flux prend fin lorsqu’on lit un `header` à `0x00`.
- Le décodeur ne connaît pas la taille finale ; c’est au code appelant de s’assurer que le buffer `dst` est assez grand.

## Bonnes pratiques d’utilisation

- **Ne pas** appeler ces fonctions à chaque frame pour un sprite :
  - Décompresser les sprites en RAM au chargement ou au spawn.
  - Travailler ensuite uniquement avec les données décompressées.
- Pour les niveaux :
  - Décompresser la tilemap **une seule fois** dans un buffer `level_buffer`.
  - Garder ce buffer comme représentation de référence.

Source: `mo5-docs/mo5/compression-rle-MO5.md`

