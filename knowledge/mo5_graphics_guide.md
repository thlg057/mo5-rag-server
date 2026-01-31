# Guide complet : Dessiner en graphique sur Thomson MO5

## 📋 Table des matières
1. [Architecture mémoire vidéo](#architecture-mémoire-vidéo)
2. [Format des pixels](#format-des-pixels)
3. [Système de couleurs](#système-de-couleurs)
4. [Structure d'un sprite](#structure-dun-sprite)
5. [Exemples commentés](#exemples-commentés)
6. [Bonnes pratiques](#bonnes-pratiques)
7. [Code type réutilisable](#code-type-réutilisable)

---

## Architecture mémoire vidéo

### Résolution et organisation
- **Résolution** : 320×200 pixels en 4 couleurs
- **Organisation mémoire** : 40 octets × 200 lignes = 8000 octets
- **Adresse de base** : `0x0000`

### Dual-bank system (2 banques mémoire)
Le MO5 utilise **deux banques mémoire** superposées :

1. **Banque COULEUR** (Forme/Fond)
   - Définit la palette de couleurs pour chaque groupe de 4 pixels
   - Accès via `*PRC &= ~0x01;`
   
2. **Banque FORME** (Bitmap)
   - Définit quels pixels sont allumés (forme) ou éteints (fond)
   - Accès via `*PRC |= 0x01;`

### Registres importants
```c
#define PRC       ((unsigned char *)0xA7C0)  // Registre de contrôle pagination
#define VIDEO_REG ((unsigned char *)0xA7E7)  // Registre vidéo
```

### Table d'offset des lignes
Pour accélérer l'accès, précalculer les offsets :
```c
unsigned int row_offsets[200];
for (int i = 0; i < 200; i++) {
    row_offsets[i] = i * 40;  // Chaque ligne = 40 octets
}
```

---

## Format des pixels

### Structure d'un octet
**Un octet = 4 pixels de 2 bits chacun**

```
Octet:    [ 7 6 | 5 4 | 3 2 | 1 0 ]
Pixels:   [ P0  | P1  | P2  | P3  ]
Position: [gauche ←――――――――→ droite]
```

### Valeurs binaires
- `00` = pixel éteint (couleur de FOND)
- `11` = pixel allumé (couleur de FORME)

### Exemples de conversion

| Binaire | Hex | Pixels visuels | Description |
|---------|-----|----------------|-------------|
| `11111111` | `0xFF` | `████` | 4 pixels allumés |
| `00000000` | `0x00` | `----` | 4 pixels éteints |
| `11110000` | `0xF0` | `██--` | 2 allumés, 2 éteints |
| `00001111` | `0x0F` | `--██` | 2 éteints, 2 allumés |
| `11000000` | `0xC0` | `█---` | 1 allumé, 3 éteints |
| `00000011` | `0x03` | `---█` | 3 éteints, 1 allumé |
| `00111100` | `0x3C` | `-██-` | Contour fin centré |
| `11001100` | `0xCC` | `█-█-` | Pixels alternés |

### Technique de conversion manuelle

Pour convertir un motif visuel en hexadécimal :

**Exemple : dessiner `██--██--`**

1. Séparer en groupes de 2 bits : `11 00 11 00`
2. Convertir chaque paire : `11=3`, `00=0`, `11=3`, `00=0`
3. Regrouper par 4 bits : `[11 00] [11 00]` = `[C] [C]`
4. Résultat : `0xCC`

**Autre exemple : `--████--`**

1. Visuel : `--████--`
2. En bits : `00 11 11 00`
3. Groupes : `[00 11] [11 00]` = `[3] [C]`
4. Résultat : `0x3C`

---

## Système de couleurs

### Palette officielle MO5
```c
#define C_BLACK   0  // Noir
#define C_RED     1  // Rouge
#define C_GREEN   2  // Vert
#define C_YELLOW  3  // Jaune
#define C_BLUE    4  // Bleu
#define C_MAGENTA 5  // Magenta
#define C_CYAN    6  // Cyan
#define C_WHITE   7  // Blanc
```

### Format de l'attribut couleur
```c
// Le fond utilise les bits 4-6, la forme les bits 0-3
#define COLOR(bg, fg) (unsigned char)(((fg & 0x07) << 4) | (bg & 0x0F))
```

**Explication du format :**
```
Octet couleur : [ 7 | 6 5 4 | 3 | 2 1 0 ]
                [ - |  FG   | - |  BG   ]
                     Forme      Fond
```

### Exemples d'attributs
```c
COLOR(C_BLACK, C_RED)     // Rouge sur fond noir
COLOR(C_BLUE, C_YELLOW)   // Jaune sur fond bleu
COLOR(C_BLACK, C_WHITE)   // Blanc sur fond noir
```

---

## Structure d'un sprite

### Format standard : Sprite 32×32 pixels

Un sprite de 32×32 pixels = **4 octets × 32 lignes = 128 octets**

```c
unsigned char mon_sprite[128] = {
    // Ligne 0 (4 octets = 16 pixels)
    0x00, 0xFF, 0xFF, 0x00,
    
    // Ligne 1
    0x0F, 0x00, 0x00, 0xF0,
    
    // ... 30 lignes suivantes
};
```

### Processus de dessin d'un sprite

```c
void draw_sprite32(int tx, int py, unsigned char *data, unsigned char color) {
    unsigned char *vram = (unsigned char *)0x0000;
    
    for (int i = 0; i < 32; i++) {  // Pour chaque ligne
        unsigned int offset = row_offsets[py + i] + tx;
        
        // 1. Écrire la COULEUR (4 octets)
        *PRC &= ~0x01;  // Sélectionner banque COULEUR
        vram[offset]   = color; 
        vram[offset+1] = color; 
        vram[offset+2] = color; 
        vram[offset+3] = color;
        
        // 2. Écrire la FORME (4 octets)
        *PRC |= 0x01;   // Sélectionner banque FORME
        vram[offset]   = data[i*4]; 
        vram[offset+1] = data[i*4+1]; 
        vram[offset+2] = data[i*4+2]; 
        vram[offset+3] = data[i*4+3];
    }
}
```

---

## Exemples commentés

### Exemple 1 : Carré plein

```c
unsigned char sprite_carre[] = {
    // Ligne 0-1 : Bord supérieur (entièrement rempli)
    0xFF,0xFF,0xFF,0xFF,  // ████████████████
    0xFF,0xFF,0xFF,0xFF,  // ████████████████
    
    // Lignes 2-29 : Corps avec contour
    0xFF,0x00,0x00,0xFF,  // ████--------████
    0xFF,0x00,0x00,0xFF,  // ████--------████
    // ... répéter 26 fois
    
    // Lignes 30-31 : Bord inférieur
    0xFF,0xFF,0xFF,0xFF,  // ████████████████
    0xFF,0xFF,0xFF,0xFF   // ████████████████
};
```

**Explication :**
- `0xFF` = `11111111` = 4 pixels allumés = bordure pleine
- `0x00` = `00000000` = 4 pixels éteints = intérieur vide
- Les colonnes gauche et droite restent à `0xFF` pour le contour vertical

### Exemple 2 : Cercle (approximation)

```c
unsigned char sprite_rond[] = {
    0x00,0x3F,0xFC,0x00,  // 0  ----████████----
    0x00,0xFF,0xFF,0x00,  // 1  --████████████--
    0x03,0xFF,0xFF,0xC0,  // 2  ████████████████
    0x0F,0xFF,0xFF,0xF0,  // 3  ████████████████
    0x0F,0xF0,0x0F,0xF0,  // 4  ████████----████
    0x3F,0xC0,0x03,0xFC,  // 5  ██████------██████
    0x3F,0x00,0x00,0xFC,  // 6  ████----------████
    0xFF,0x00,0x00,0xFF,  // 7  ████----------████
    // ... milieu du cercle (lignes 8-23)
    0xFF,0x00,0x00,0xFF,  // 24 ████----------████
    0x3F,0x00,0x00,0xFC,  // 25 ████----------████
    0x3F,0xC0,0x03,0xFC,  // 26 ██████------██████
    0x0F,0xF0,0x0F,0xF0,  // 27 ████████----████
    0x0F,0xFF,0xFF,0xF0,  // 28 ████████████████
    0x03,0xFF,0xFF,0xC0,  // 29 ████████████████
    0x00,0xFF,0xFF,0x00,  // 30 --████████████--
    0x00,0x3F,0xFC,0x00   // 31 ----████████----
};
```

**Détail des octets pour la ligne 0 :**
- `0x00` = `00000000` = `----` (4 pixels vides à gauche)
- `0x3F` = `00111111` = `-███` (2 pixels vides, 2 allumés)
- `0xFC` = `11111100` = `███-` (6 pixels allumés, 2 vides)
- `0x00` = `00000000` = `----` (4 pixels vides à droite)

### Exemple 3 : Triangle

```c
unsigned char sprite_triangle[] = {
    0x00,0x03,0xC0,0x00,  // 0  ------██------
    0x00,0x03,0xC0,0x00,  // 1  ------██------
    0x00,0x0F,0xF0,0x00,  // 2  ----██████----
    0x00,0x0F,0xF0,0x00,  // 3  ----██████----
    0x00,0x3C,0x3C,0x00,  // 4  --████--████--
    0x00,0x3C,0x3C,0x00,  // 5  --████--████--
    0x00,0xF0,0x0F,0x00,  // 6  ████------████
    0x00,0xF0,0x0F,0x00,  // 7  ████------████
    // ... élargissement progressif
    0xFF,0xFF,0xFF,0xFF,  // 28 ████████████████ (base)
    0xFF,0xFF,0xFF,0xFF,  // 29 ████████████████
    0xFF,0xFF,0xFF,0xFF,  // 30 ████████████████
    0xFF,0xFF,0xFF,0xFF   // 31 ████████████████
};
```

**Construction du contour du triangle :**
- Ligne 0 : `0x03` = `00000011` = pointe fine de 2 pixels
- Ligne 4 : `0x3C` = `00111100` = contours espacés
- Ligne 6 : `0xF0` et `0x0F` = contours très espacés
- Lignes 28-31 : `0xFF` = base pleine

---

## Bonnes pratiques

### 1. Organisation du code

```c
// Toujours définir ces constantes
#define PRC       ((unsigned char *)0xA7C0)
#define VIDEO_REG ((unsigned char *)0xA7E7)

// Toujours précalculer les offsets
unsigned int row_offsets[200];
```

### 2. Initialisation propre

```c
void init_all() {
    int i;
    
    // 1. Calculer les offsets
    for (i = 0; i < 200; i++) {
        row_offsets[i] = i * 40;
    }
    
    // 2. Initialiser les registres
    *PRC = 0x00;
    *VIDEO_REG |= 0x01;
    
    // 3. Nettoyer l'écran (IMPORTANT)
    for (i = 0; i < 8000; i++) {
        *PRC &= ~0x01;  // Banque COULEUR
        ((unsigned char*)0x0000)[i] = COLOR(C_BLACK, C_BLACK);
        
        *PRC |= 0x01;   // Banque FORME
        ((unsigned char*)0x0000)[i] = 0x00;
    }
}
```

### 3. Ordre des opérations CRITIQUE

**Toujours dessiner dans cet ordre :**
1. ✅ D'abord écrire la COULEUR (`*PRC &= ~0x01`)
2. ✅ Ensuite écrire la FORME (`*PRC |= 0x01`)

**Pourquoi ?** Si vous écrivez la forme avant la couleur, vous verrez des artefacts gris/parasites à l'écran.

### 4. Effacement de sprites

```c
void clear_sprite32(int tx, int py) {
    unsigned char *vram = (unsigned char *)0x0000;
    
    for (int i = 0; i < 32; i++) {
        unsigned int offset = row_offsets[py + i] + tx;
        *PRC |= 0x01;  // Banque FORME uniquement
        vram[offset]   = 0x00;
        vram[offset+1] = 0x00;
        vram[offset+2] = 0x00;
        vram[offset+3] = 0x00;
    }
}
```

### 5. Positionnement des sprites

**Coordonnées :**
- `tx` (X) : de 0 à 36 (40 octets - 4 octets du sprite)
- `py` (Y) : de 0 à 168 (200 lignes - 32 lignes du sprite)

**Centrage horizontal :**
```c
int center_x = (40 - 4) / 2;  // = 18 octets
```

**Centrage vertical :**
```c
int center_y = (200 - 32) / 2;  // = 84 lignes
```

---

## Code type réutilisable

### Programme complet minimal

```c
#include <cmoc.h>

#define PRC       ((unsigned char *)0xA7C0)
#define VIDEO_REG ((unsigned char *)0xA7E7)

#define C_BLACK   0
#define C_RED     1
#define C_GREEN   2
#define C_YELLOW  3
#define C_BLUE    4
#define C_MAGENTA 5
#define C_CYAN    6
#define C_WHITE   7

#define COLOR(bg, fg) (unsigned char)(((fg & 0x07) << 4) | (bg & 0x0F))

unsigned int row_offsets[200];

// Votre sprite ici
unsigned char mon_sprite[128] = {
    // 32 lignes × 4 octets
};

void draw_sprite32(int tx, int py, unsigned char *data, unsigned char color) {
    unsigned char *vram = (unsigned char *)0x0000;
    for (int i = 0; i < 32; i++) {
        unsigned int offset = row_offsets[py + i] + tx;
        *PRC &= ~0x01; 
        vram[offset] = color; 
        vram[offset+1] = color; 
        vram[offset+2] = color; 
        vram[offset+3] = color;
        *PRC |= 0x01;  
        vram[offset] = data[i*4]; 
        vram[offset+1] = data[i*4+1]; 
        vram[offset+2] = data[i*4+2]; 
        vram[offset+3] = data[i*4+3];
    }
}

void init_all() {
    for (int i = 0; i < 200; i++) row_offsets[i] = i * 40;
    *PRC = 0x00;
    *VIDEO_REG |= 0x01;
    for (unsigned int i = 0; i < 8000; i++) {
        *PRC &= ~0x01; 
        ((unsigned char*)0x0000)[i] = COLOR(C_BLACK, C_BLACK);
        *PRC |= 0x01;  
        ((unsigned char*)0x0000)[i] = 0x00;
    }
}

int main() {
    init_all();
    
    // Dessiner votre sprite au centre
    draw_sprite32(18, 84, mon_sprite, COLOR(C_BLACK, C_RED));
    
    while(1);  // Boucle infinie
    return 0;
}
```

---

## Technique avancée : Dessin pixel par pixel

Si vous avez besoin de dessiner pixel par pixel (moins efficace mais plus flexible) :

```c
void set_pixel(int x, int y, unsigned char color) {
    if (x < 0 || x >= 160 || y < 0 || y >= 200) return;
    
    unsigned char *vram = (unsigned char *)0x0000;
    unsigned int offset = row_offsets[y] + (x / 4);  // 4 pixels par octet
    unsigned char shift = (3 - (x & 3)) * 2;         // Position du pixel (0,2,4,6)
    unsigned char mask = 0x03 << shift;              // Masque 2 bits
    
    *PRC &= ~0x01;  // Banque COULEUR
    // On pourrait aussi changer la couleur ici si besoin
    
    *PRC |= 0x01;   // Banque FORME
    unsigned char current = vram[offset];
    vram[offset] = (current & ~mask) | (0x03 << shift);  // Allumer le pixel
}
```

---

## Checklist de débogage

❌ **Problème : Écran noir**
- ✅ Vérifier que `*VIDEO_REG |= 0x01;` est appelé
- ✅ Vérifier que les sprites ne sont pas remplis de `0x00`
- ✅ Vérifier les coordonnées (ne pas dépasser l'écran)

❌ **Problème : Artefacts gris/parasites**
- ✅ Nettoyer FORME après avoir écrit COULEUR dans `init_all()`
- ✅ Toujours écrire COULEUR avant FORME dans les fonctions de dessin

❌ **Problème : Mauvaises couleurs**
- ✅ Vérifier la macro `COLOR(bg, fg)` 
- ✅ Vérifier que les valeurs sont entre 0-7

❌ **Problème : Sprite déformé**
- ✅ Vérifier que chaque ligne fait exactement 4 octets
- ✅ Compter qu'il y a bien 32 lignes (128 octets total)
- ✅ Vérifier la conversion binaire→hex

---

## Résumé ultra-rapide

```
🎨 PIXELS : 1 octet = 4 pixels de 2 bits
📐 SPRITE : 32×32 = 4 octets × 32 lignes = 128 octets
🏦 BANQUES : Couleur (*PRC &= ~0x01) puis Forme (*PRC |= 0x01)
🎯 POSITION : tx (0-36), py (0-168)
🖌️ ORDRE : TOUJOURS Couleur AVANT Forme !

Conversion rapide :
██ = 11 = 0xC0 (si à gauche)
-- = 00 = 0x00
████ = 11111111 = 0xFF
```

---

**Document créé pour faciliter l'apprentissage du dessin graphique sur Thomson MO5 avec le compilateur CMOC.**

*Version 1.0 - Janvier 2025*