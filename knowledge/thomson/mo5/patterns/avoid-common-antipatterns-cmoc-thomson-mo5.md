# Avoid Common Anti-Patterns (Thomson MO5 / CMOC)

Liste des erreurs fréquentes à éviter lors du développement sur Thomson MO5 avec cmoc.

## Anti-patterns hardware

```text
❌ Supposer un mapping couleur RGB linéaire
   → Le MO5 utilise une PROM analogique non linéaire. Tester sur émulateur.

❌ Écrire en VRAM sans sélectionner la bonne banque
   → Toujours positionner *PRC avant toute écriture VRAM.

❌ Supposer E=1/E=0 comme sur TO7
   → Le MO5 utilise un gate-array custom, pas un Motorola 6846.

❌ Accès mémoire dispersés pendant l'affichage actif
   → Grouper les accès, privilégier le VBL pour les gros transferts.

❌ Framebuffer linéaire complet
   → Pas assez de RAM. Utiliser dirty rectangles.
```

## Anti-patterns cmoc / C89

```text
❌ Déclarer une variable après un statement dans un bloc
   → Comportement indéfini au runtime, sans erreur de compilation.

❌ Initialiser une variable à sa déclaration (unsigned char x = 0)
   → Peut crasher avec cmoc. Séparer déclaration et initialisation.

❌ Déclarer mo5_wait_vbl() en static inline
   → Le compilateur peut mettre en cache la valeur du registre VBL,
     rendant la boucle d'attente infinie.

❌ Trop de variables locales dans une fonction
   → Stack overflow silencieux : le programme freeze à l'entrée
     de la fonction. Promouvoir en static global (gl_ prefix).
```

## Anti-patterns performance

```text
❌ Utiliser int là où unsigned char suffit
   → Opérations 16-bit sur un CPU 8-bit = 2-4x plus lent.

❌ Utiliser des divisions à l'exécution
   → Pas de division hardware sur 6809. Utiliser >> ou lookup tables.

❌ Calculer y * 40 à l'exécution
   → Utiliser la table row_offsets[] précalculée au démarrage.

❌ Utiliser malloc
   → Pas disponible efficacement sur MO5. Tout est statique.
```

Source: `mo5_hardware_reference.md` section 9
