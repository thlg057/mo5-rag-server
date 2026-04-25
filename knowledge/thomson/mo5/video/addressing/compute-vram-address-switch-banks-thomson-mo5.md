# Compute VRAM Address and Switch Banks (Thomson MO5)

La VRAM du MO5 est organisée en deux plans superposés (FORME et COULEUR) accessibles via bank-switch. Toute écriture VRAM nécessite de sélectionner la bonne banque au préalable.

## Organisation VRAM

- Résolution : 320×200 pixels
- 1 octet = 8 pixels horizontaux
- 40 octets par ligne
- Base adresse : `$0000`

## Deux plans mémoire

| Plan | Rôle | Sélection via `$A7C0` bit 0 |
|------|------|-----------------------------|
| FORME | 1 bit par pixel (allumé/éteint) | bit 0 = 0 |
| COULEUR | 4 bits d'attribut couleur | bit 0 = 1 |

## Sélection de banque

```c
#define PRC  ((unsigned char *)0xA7C0)

*PRC &= ~0x01;   /* sélectionner banque COULEUR */
*PRC |=  0x01;   /* sélectionner banque FORME   */
```

## Calcul d'adresse pixel

```c
addr = y * 40 + (x >> 3);   /* octet contenant le pixel */
mask = 0x80 >> (x & 7);     /* masque du bit dans l'octet */
```

## Optimisation critique — table row_offsets

Ne jamais calculer `y * 40` à l'exécution sur 6809 (multiplication coûteuse).
Utiliser une table précalculée une seule fois au démarrage :

```c
unsigned int row_offsets[200];

/* init (une seule fois) */
for (i = 0; i < 200; i++)
    row_offsets[i] = (unsigned int)i * 40;

/* usage dans le code d'affichage */
addr = row_offsets[y] + (x >> 3);
```

## Règle critique d'écriture pixel

```text
1. Sélectionner FORME  (*PRC &= ~0x01)
2. Modifier le bit      (VRAM[addr] |= mask)
3. Sélectionner COULEUR (*PRC |= 0x01)
4. Écrire l'attribut    (VRAM[addr] = color)
```

> ⚠️ Ne jamais écrire en COULEUR sans avoir sélectionné la bonne banque.

Source: `mo5_hardware_reference.md` section 3
