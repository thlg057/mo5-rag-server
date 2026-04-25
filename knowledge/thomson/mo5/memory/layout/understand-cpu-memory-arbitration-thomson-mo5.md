# Understand CPU Memory Arbitration (Thomson MO5)

Le MO5 utilise un gate-array custom pour arbitrer les accès mémoire entre le CPU et le circuit vidéo. Le CPU n'a pas accès continu à la RAM.

## Matériel

- CPU : Motorola 6809E à ~1 MHz
- RAM partagée entre CPU et vidéo
- Arbitrage géré par **gate-array custom** (≠ TO7 qui utilise un Motorola 6846)

## Modèle réel

```text
MEMORY ACCESS IS ARBITRATED
- CPU access is NOT continuous
- Video hardware performs periodic reads
- Gate-array schedules memory cycles
- Video priority > CPU priority
```

> ❌ NE PAS supposer : E=1 → CPU / E=0 → VIDEO
> Ce modèle est celui du TO7, **pas du MO5**

## Conséquences pratiques

- Latence mémoire variable
- Boucles serrées = ralentissement possible pendant affichage actif
- Privilégier les accès pendant le VBL (blanking vertical)
- Grouper les écritures RAM en rafales courtes

## Notes

- Le gate-array génère RAS/CAS pour la DRAM, multiplex adresses CPU/vidéo
- La priorité vidéo est absolue — le CPU attend si conflit

Source: `mo5_hardware_reference.md` section 1
