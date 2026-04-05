# Règles de déclaration de variables avec cmoc (MO5)

## Le problème

cmoc est un compilateur C ciblant le Motorola 6809 (Thomson MO5/TO8). Il compile du **C89/C90** — pas du C99 ni du C++ — et applique une règle stricte :

> **Toutes les déclarations de variables doivent se trouver au tout début d'un bloc, avant tout autre statement.**

Si une déclaration apparaît après un appel de fonction, une affectation, ou n'importe quelle autre instruction, le compilateur peut générer du code incorrect **sans forcément émettre d'erreur ou de warning visible**. Le programme se compile, mais le comportement au runtime est imprévisible : variables avec des valeurs aléatoires, corruption mémoire, instructions ignorées.

---

## Ce qui ne fonctionne pas (C99, interdit en C89)

```c
void game_loop(void)
{
    unsigned char score = 0;

    game_init_player();       // ← statement
    game_init_enemies();      // ← statement

    unsigned char new_x = player.pos.x;  // ← ERREUR : déclaration après un statement
    char key;                             // ← ERREUR : idem
}
```

```c
for (i = 0; i < COUNT; i++) {
    if (!actors[i].active) continue;     // ← statement

    unsigned char new_x = actors[i].pos.x;  // ← ERREUR : déclaration après un statement
    unsigned char new_y = actors[i].pos.y;
}
```

---

## Ce qu'il faut faire (C89, correct)

Toutes les déclarations en tête de bloc, **avant** le premier statement :

```c
void game_loop(void)
{
    unsigned char score = 0;
    unsigned char new_x;     // ← déclaré ici, sans initialisation
    char key;                // ← déclaré ici

    game_init_player();      // statements après les déclarations
    game_init_enemies();

    new_x = player.pos.x;   // ← initialisation ici
}
```

```c
static void game_update_enemies(void)
{
    unsigned char i;
    unsigned char need_reverse = 0;
    unsigned char new_x, new_y;    // ← déclarés en tête de fonction

    for (i = 0; i < COUNT; i++) {
        if (!actors[i].active) continue;

        new_x = actors[i].pos.x;   // ← affectation, pas déclaration
        new_y = actors[i].pos.y;
    }
}
```

---

## Règle pratique

Pour chaque fonction ou bloc `{}`, applique cette structure :

```
{
    /* 1. Toutes les déclarations de variables */
    /* 2. Toutes les initialisations / statements */
    /* 3. La logique du code */
}
```

---

## Pourquoi c'est difficile à détecter

- cmoc ne génère pas toujours une erreur explicite
- Le code se compile sans problème apparent
- Le bug se manifeste de façon aléatoire selon la position en mémoire des variables mal placées : certaines fonctions semblent fonctionner, d'autres non
- Les symptômes peuvent ressembler à un bug logique (entrées ignorées, valeurs corrompues) plutôt qu'à un problème de compilation

---

## Checklist avant de compiler

- [ ] Aucune déclaration de variable après un appel de fonction dans le même bloc
- [ ] Aucune déclaration de variable après une affectation dans le même bloc
- [ ] Dans les boucles `for`, les variables utilisées à l'intérieur sont déclarées avant la boucle
- [ ] `char key`, `unsigned char new_x`, etc. sont tous en tête de leur fonction
