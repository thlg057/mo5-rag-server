# Checklist de démarrage d’un projet Thomson MO5 avec CMOC

Ce chunk propose une courte checklist pour initialiser proprement un projet MO5 avec CMOC.

## Goal

- Avoir un squelette minimal pour un projet C CMOC ciblant le MO5.
- Éviter les oublis récurrents (headers, redirection de sortie, assertions…).

## 1. Inclure les bons headers

```c
#include <cmoc.h>      // Bibliothèque standard CMOC
#include <stdarg.h>    // Si fonctions variadiques (inclus par cmoc.h)
#include <assert.h>    // Assertions de debug
```

## 2. Définir des types de base adaptés au MO5

Inspire‑toi de `<vectrex.h>` pour créer ton propre `mo5.h` :

```c
// mo5.h
typedef unsigned char  byte;
typedef signed char    sbyte;
typedef unsigned int   word;
typedef signed int     sword;
typedef unsigned long  uint32_t;
typedef signed long    int32_t;
enum { FALSE = 0, TRUE = 1 };
```

Inclure ce header dans tout ton projet :

```c
#include "mo5.h"
```

## 3. Rediriger la sortie console de CMOC vers l’écran MO5

Au début de `main()` :

```c
extern void maRoutineAffichageMO5(void);

int main(void) {
    setConsoleOutHook(maRoutineAffichageMO5);
    // ...
}
```

`maRoutineAffichageMO5` reçoit le caractère à afficher dans le registre A et doit préserver au moins B, X, Y, U.

## 4. Utiliser les assertions en debug

```c
#include <assert.h>

int main(void) {
    _SetFailedAssertHandler(monHandlerMO5);
    // ...
}
```

Installe un handler qui affiche un message sur l’écran MO5 et se met éventuellement en boucle infinie pour laisser le temps de lire.

## 5. Désactiver les assertions en release

En build “release”, compile avec `NDEBUG` :

```c
#define NDEBUG
#include <assert.h>
```

Toutes les `assert()` deviennent alors des no‑ops sans coût à l’exécution.

Source: `mo5-docs/cmoc/cmoc.md`
