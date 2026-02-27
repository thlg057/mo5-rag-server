# Concevoir des sprites logiciels performants sur Thomson MO5

## Goal

Donner des lignes directrices pour concevoir des sprites logiciels adaptés aux contraintes CPU du MO5.

## Pas de sprites hardware

Le MO5 ne dispose **d’aucun sprite matériel** :
- tous les sprites sont dessinés en **logiciel** dans la VRAM,
- chaque déplacement implique des lectures/écritures mémoire coûteuses.

## Recommandations principales

1. **Utiliser des masques (AND/OR)** pour la transparence :
   - préserver le fond sans le recalculer,
   - appliquer la forme du sprite par opérations logiques.
2. **Aligner les sprites sur des frontières de 8 pixels** :
   - un octet = 8 pixels horizontaux,
   - l’alignement évite des décalages de bits coûteux.
3. Si un décalage au pixel près est nécessaire :
   - pré-calculer les **8 versions décalées** du sprite en ROM/RAM,
   - choisir la bonne version selon la position.
4. **Limiter le nombre de sprites actifs** :
   - typiquement 5–10 sprites moyens par frame selon leur taille,
   - au-delà, le budget CPU peut être dépassé.

## Lien avec le budget CPU

- À 1 MHz et 50 Hz, une frame offre ~20 000 cycles.
- Chaque sprite dessiné consomme une portion significative de ce budget.
- Les recommandations ci-dessus permettent de :
  - réduire les lectures/écritures VRAM,
  - éviter des boucles de décalage de bits.

Source: `mo5-docs/mo5/guide-developpement-MO5.md`

