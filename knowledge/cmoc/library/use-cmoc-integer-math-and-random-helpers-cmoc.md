# Utiliser les helpers math entiers et aléatoires de CMOC

Ce chunk regroupe les fonctions d’aide pour les maths entières 16/32 bits et le générateur pseudo‑aléatoire.

## Goal

- Exploiter les fonctions de maths intégrées à CMOC au lieu de réinventer la roue.
- Implémenter des mécaniques de jeu (physique simple, RNG) sur MO5.

## Valeur absolue

- `int abs(int j)`
- `long int labs(long int j)`

## Racines carrées entières

- `unsigned char sqrt16(unsigned short n)`
- `unsigned short sqrt32(unsigned long n)`

Utile pour des distances approximatives, effets, etc., sans passer par la virgule flottante.

## Divisions optimisées

- `void divmod16(unsigned dividend, unsigned divisor, unsigned *quotient, unsigned *remainder)`
- `void divmod8(unsigned char dividend, unsigned char divisor, unsigned char *quotient, unsigned char *remainder)`
- `void divdwb(unsigned dividendInQuotientOut[2], unsigned char divisor)`
- `void divdww(unsigned dividendInQuotientOut[2], unsigned divisor)`

`divmod*` calcule quotient et reste en une seule passe, adapté au 6809.

## Multiplications 16/32 bits

- `unsigned mulwb(unsigned char *hi, unsigned wordFactor, unsigned char byteFactor)`
- `unsigned mulww(unsigned *hi, unsigned factor0, unsigned factor1)`

Ces fonctions renvoient la partie basse du résultat et placent la partie haute derrière le pointeur passé.

## Arithmétique 32 bits

- `void zerodw(unsigned *twoWords)`
- `void adddww(unsigned *twoWords, unsigned term)`
- `void subdww(unsigned *twoWords, unsigned term)`
- `signed char cmpdww(unsigned left[2], unsigned right)`

Pratique pour des compteurs ou distances 32 bits sans gérer manuellement les retenues.

## Générateur pseudo‑aléatoire

- `void srand(unsigned seed)`
- `int rand(void)` (retourne dans `[0, 0x7FFF]`)

Pattern typique :

```c
srand(1);          // seed fixe pour des tests reproductibles
int r = rand();    // 0..0x7FFF
```

Sur MO5, tu peux dériver une valeur de seed d’une saisie utilisateur ou d’une variable “bruitée” (par exemple nombre de frames écoulées avant un input).

Source: `mo5-docs/cmoc/cmoc_h.md`
