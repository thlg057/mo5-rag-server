# Implémenter un encodeur RLE en Python pour les assets MO5

## Goal

Écrire un script Python côté PC qui compresse les données binaires (sprites, niveaux, etc.) en RLE avant de les inclure dans un jeu Thomson MO5.

## RLE simple `(count, value)`

Encodeur minimal :

```python
def rle_encode(data: bytes) -> bytes:
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
```

Caractéristiques :
- Très simple à décodeur côté MO5.
- Efficace uniquement si les données contiennent beaucoup de répétitions longues.

## RLE avec mode littéral (recommandé)

Pour de meilleures performances sur des données mixtes (zones répétitives + détails), on utilise un format avec deux modes :

- **Répétition** : bit7 du compteur = 0.
- **Littéral** : bit7 du compteur = 1.

Pseudo-encodeur :

```python
def rle_encode_with_literal(data: bytes) -> bytes:
    """RLE avec mode littéral (bit7 = 1). Plus efficace sur données variées."""
    result = []
    i = 0
    while i < len(data):
        # 1) Chercher une séquence répétée
        value = data[i]
        run = 1
        while i + run < len(data) and data[i + run] == value and run < 127:
            run += 1

        if run >= 3:
            # Répétition rentable → encode en mode répétition
            result.append(run)          # bit7 = 0
            result.append(value)
            i += run
        else:
            # 2) Sinon, construire un bloc littéral
            lit = []
            while i < len(data):
                v = data[i]
                r = 1
                while i + r < len(data) and data[i + r] == v and r < 3:
                    r += 1
                if r >= 3 or len(lit) >= 127:
                    break
                lit.append(v)
                i += r
            result.append(0x80 | len(lit))  # bit7 = 1 → littéral
            result.extend(lit)

    result.append(0x00)  # token de fin
    return bytes(result)
```

Points importants :
- Les répétitions de longueur ≥ 3 sont encodées en mode répétition.
- Les petites variations (runs de longueur 1 ou 2) sont agrégées dans un bloc littéral.
- Un octet `0x00` final sert de **fin de flux**.

## Intégration dans le pipeline

Ce code tourne sur la machine de développement (PC) et s’intègre typiquement dans un script `convert_assets.py` qui :

1. Charge des fichiers bruts (PNG convertis, niveaux texte, etc.).
2. Les transforme en blocs binaires adaptés au MO5.
3. Applique l’encodeur RLE.
4. Génère des fichiers `.h` ou `.bin` à inclure dans le projet C.

Source: `mo5-docs/mo5/compression-rle-MO5.md`

