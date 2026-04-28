# Compression RLE sur Thomson MO5

> Pourquoi, quand et comment compresser ses assets sur une machine à mémoire limitée

---

## 1. Le problème

Sur MO5, la mémoire disponible pour stocker les assets d'un jeu est très contrainte. Un inventaire typique peut vite dépasser ce qui rentre :

| Asset | Taille brute |
|---|---|
| 1 sprite 16×16 (forme + couleur) | 64 octets |
| 50 sprites | 3 200 octets |
| 1 niveau (tilemap 40×25) | 1 000 octets |
| 10 niveaux | 10 000 octets |
| Musique + effets | 1 000–5 000 octets |
| **Total** | **~15–20 Ko** |

Sur 48 Ko dont une bonne partie prise par le système et le code, les assets peuvent représenter un tiers ou plus de la mémoire disponible. La compression permet d'en récupérer une partie significative.

---

## 2. Principe du RLE (Run-Length Encoding)

L'idée est simple : au lieu de stocker chaque octet individuellement, on stocke les **répétitions**.

### Format classique : `(count, value)`

```
Données brutes  →  RLE encodé
AA AA AA AA AA  →  05 AA        ("répéter AA 5 fois")
1B 2C 3D        →  01 1B 01 2C 01 3D  (pas de répétition → overhead)
```

### Exemple concret sur une ligne d'écran MO5

Une ligne de ciel (40 bytes de zéros) :

```
Brut    : 00 00 00 00 00 00 00 00 ... (×40)  = 40 octets
RLE     : 28 00                              =  2 octets
Gain    : 95%
```

Une ligne de sol avec un motif répétitif :

```
Brut    : FF 00 FF 00 FF 00 FF 00 ... (×20 paires) = 40 octets
RLE     : 01 FF 01 00 01 FF 01 00 ...               = 80 octets  ← PIRE !
```

> **Règle d'or** : RLE est efficace sur les **grandes zones uniformes**, contre-productif sur les données variées.

---

## 3. Variantes du format RLE

### Variante 1 : RLE simple `(count, value)`

La plus facile à implémenter. Chaque paire d'octets = une répétition.

```
Encodage : [count] [value] [count] [value] ...
Exemple  : 05 AA 03 BB 01 CC
Décode   : AA AA AA AA AA BB BB BB CC
```

**Avantage** : trivial à décoder.  
**Inconvénient** : les séquences non répétitives coûtent 2 octets par valeur au lieu de 1.

### Variante 2 : RLE avec flag de mode (recommandée)

Un bit du compteur indique si la séquence est une **répétition** ou une **copie littérale**.

```
Si bit7 du compteur = 0 → répétition  : [0b0_count] [value]
Si bit7 du compteur = 1 → littéral   : [0b1_count] [val1] [val2] ...
```

Exemple :
```
Encodage : 05 AA  84 1B 2C 3D 4E  03 FF
Décode   : AA AA AA AA AA  1B 2C 3D 4E  FF FF FF
```

**Avantage** : les séquences non répétitives sont stockées sans overhead (1 octet de header pour jusqu'à 127 valeurs).  
**Inconvénient** : décodeur légèrement plus complexe.

### Variante 3 : token de fin

Ajouter un token de fin de données pour simplifier le décodeur :

```
00 00  →  fin de stream  (count=0 value=0, jamais émis autrement)
```

---

## 4. Implémentation sur MO5

### Encodeur (sur PC, en Python)

L'encodeur tourne sur ta machine de développement, pas sur le MO5.

```python
def rle_encode(data):
    """RLE simple (count, value). Count limité à 255."""
    result = []
    i = 0
    while i < len(data):
        value = data[i]
        count = 1
        while i + count < len(data) and data[i + count] == value and count < 255:
            count += 1
        result.append(count)
        result.append(value)
        i += count
    return bytes(result)

def rle_encode_with_literal(data):
    """RLE avec mode littéral (bit7 = mode). Plus efficace sur données variées."""
    result = []
    i = 0
    while i < len(data):
        # Compter la répétition
        value = data[i]
        run = 1
        while i + run < len(data) and data[i + run] == value and run < 127:
            run += 1

        if run >= 3:
            # Vaut la peine d'encoder en répétition
            result.append(run)          # bit7 = 0 → répétition
            result.append(value)
            i += run
        else:
            # Collecter une séquence littérale
            lit_start = i
            lit = []
            while i < len(data):
                v = data[i]
                r = 1
                while i + r < len(data) and data[i + r] == v and r < 3:
                    r += 1
                if r >= 3:
                    break
                lit.append(v)
                i += r
                if len(lit) >= 127:
                    break
            result.append(0x80 | len(lit))  # bit7 = 1 → littéral
            result.extend(lit)

    result.append(0x00)  # token de fin
    return bytes(result)
```

### Décodeur en C (tourne sur le MO5)

```c
/* RLE simple (count, value) */
void rle_decode(const unsigned char *src, unsigned char *dst, int dst_size) {
    int out = 0;
    while (out < dst_size) {
        unsigned char count = *src++;
        unsigned char value = *src++;
        if (count == 0) break;  // token de fin
        while (count-- && out < dst_size)
            dst[out++] = value;
    }
}

/* RLE avec mode littéral (bit7 = mode) */
void rle_decode_ex(const unsigned char *src, unsigned char *dst) {
    unsigned char header;
    unsigned char count;
    unsigned char value;
    unsigned char i;
    unsigned char *out;

    out = dst;
    while ((header = *src++) != 0x00) {
        if (header & 0x80) {
            /* Mode littéral : copier count octets tels quels */
            count = header & 0x7F;
            for (i = 0; i < count; i++)
                *out++ = *src++;
        } else {
            /* Mode répétition : répéter la valeur count fois */
            count = header;
            value = *src++;
            for (i = 0; i < count; i++)
                *out++ = value;
        }
    }
}
```

### Intégration dans le pipeline d'affichage

```c
/* Buffers statiques (pas de malloc !) */
static unsigned char sprite_form_buf[16 * 2];    /* 16×16, 2 bytes/ligne */
static unsigned char sprite_color_buf[16 * 2];

/* Au moment d'afficher un sprite compressé */
void draw_sprite_compressed(int tx, int ty,
                            const unsigned char *form_rle,
                            const unsigned char *color_rle)
{
    rle_decode_ex(form_rle,  sprite_form_buf);
    rle_decode_ex(color_rle, sprite_color_buf);
    mo5_draw_sprite2(tx, ty, sprite_form_buf, sprite_color_buf, 2, 16);
}

/* Pour un niveau : décompresser une fois en RAM au chargement */
static unsigned char level_buffer[40 * 25];

void load_level(const unsigned char *level_rle) {
    rle_decode_ex(level_rle, level_buffer);
    /* Ensuite on travaille directement depuis level_buffer */
}
```

---

## 5. Ce qui se compresse bien vs mal

| Type de données | Compressibilité RLE | Pourquoi |
|---|---|---|
| Fond de ciel / mer | ⭐⭐⭐⭐⭐ | Grandes zones uniformes |
| Tilemaps de niveaux | ⭐⭐⭐⭐ | Beaucoup de tiles répétés |
| Sprites avec fond transparent | ⭐⭐⭐ | Zones de 0x00 fréquentes |
| Musique / séquences | ⭐⭐⭐ | Notes répétées, silences |
| Sprites détaillés | ⭐⭐ | Peu de répétitions |
| Données totalement aléatoires | ✗ | RLE dégrade les performances |

---

## 6. Coût du décodage à runtime

Le décodeur a un coût CPU. Il ne faut **pas** décompresser un sprite à chaque frame.

| Moment | Stratégie |
|---|---|
| **Chargement de niveau** | Décompresser tout le niveau en RAM une fois |
| **Spawn d'un ennemi** | Décompresser le sprite dans un buffer dédié |
| **Chaque frame** | ❌ Ne jamais décompresser à chaque frame |

Pour les sprites fréquemment utilisés (joueur, ennemis communs), **garder les données décompressées en RAM** en permanence. Ne compresser en ROM que les données rarement chargées.

---

## 7. Workflow de développement recommandé

```
Assets bruts (PNG, fichiers texte niveaux)
        ↓
  Outil sur PC (Python)
  → conversion en binaire MO5
  → encodage RLE
  → génération de fichiers .h ou .bin
        ↓
  Code C sur MO5
  → include des données compressées
  → décompression à la demande en RAM statique
  → affichage depuis les buffers RAM
```

Exemple de données générées automatiquement :

```c
/* Généré par convert_assets.py — ne pas éditer manuellement */
const unsigned char sprite_hero_form_rle[] = {
    0x02, 0x00, 0x04, 0xFF, 0x02, 0x00, 0x00
};
const unsigned char sprite_hero_color_rle[] = {
    0x08, 0x12, 0x00
};
```

---

## 8. Alternatives au RLE

Si RLE ne suffit pas (données trop variées), d'autres algorithmes légers existent, par ordre de complexité croissante :

| Algorithme | Gain typique | Complexité décodeur | Adapté MO5 ? |
|---|---|---|---|
| RLE simple | 30–70% sur zones uniformes | Trivial | ✅ Oui |
| RLE avec littéraux | 40–60% en général | Simple | ✅ Oui |
| LZ77 / LZSS | 50–70% | Modéré | ⚠️ Coût CPU plus élevé |
| Huffman | 40–60% | Complexe | ❌ Trop lourd pour le MO5 |

Pour la grande majorité des jeux MO5, **RLE avec littéraux suffit largement**.

---

*Ce document fait partie du guide de développement Thomson MO5.*
