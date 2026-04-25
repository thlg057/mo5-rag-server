# Declare Variables Correctly in CMOC C89 (Thomson MO5)

cmoc compile du C89/C90 strict. Les déclarations de variables mal placées provoquent des comportements indéfinis au runtime sans erreur de compilation visible.

## Règle 1 — Déclarations en tête de bloc

Toutes les déclarations doivent apparaître **avant tout statement** dans un bloc.

```c
/* ❌ INTERDIT — déclaration après un statement */
void f(void) {
    game_init();
    unsigned char x;   /* comportement indéfini, crash possible */
}

/* ✅ CORRECT */
void f(void) {
    unsigned char x;   /* déclarations en premier */
    game_init();       /* statements ensuite */
    x = 42;
}
```

## Règle 2 — Ne pas initialiser à la déclaration

```c
/* ❌ peut crasher avec cmoc */
unsigned char x = 42;
unsigned char score = 0;

/* ✅ correct */
unsigned char x;
unsigned char score;
x = 42;
score = 0;
```

## Règle dans les boucles

```c
/* ❌ variable déclarée après le début de la boucle */
for (i = 0; i < n; i++) {
    game_update();
    unsigned char tmp = actors[i].pos.x;  /* INTERDIT */
}

/* ✅ déclarée avant la boucle */
unsigned char tmp;
for (i = 0; i < n; i++) {
    game_update();
    tmp = actors[i].pos.x;
}
```

## Pourquoi c'est difficile à détecter

- cmoc ne génère pas toujours une erreur ou un warning visible
- Le programme se compile normalement
- Le comportement au runtime est imprévisible : valeurs aléatoires, entrées ignorées, crash
- Les symptômes ressemblent à des bugs logiques, pas à des bugs de compilation

## Checklist avant compilation

- [ ] Aucune déclaration après un appel de fonction dans le même bloc
- [ ] Aucune déclaration après une affectation dans le même bloc
- [ ] Toutes les variables de boucle déclarées avant la boucle
- [ ] Aucune initialisation à la déclaration (`unsigned char x = 0` interdit)

Source: `mo5_hardware_reference.md` section 7 — Fiabilité ÉLEVÉE (observé en pratique)
