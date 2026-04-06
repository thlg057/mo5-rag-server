# Spécification : makefd.py
## Générateur d'image disquette .fd bootable pour Thomson MO5

---

## Contexte et objectif

Ce script Python remplace l'outil C `fdfs -addBL` du projet
[BootFloppyDisk d'OlivierP-To8](https://github.com/OlivierP-To8/BootFloppyDisk).

**Commande remplacée :**
```
fdfs -addBL output.fd BOOTMO.BIN program.BIN [file2.BIN ...]
```

**Commande cible :**
```
python3 makefd.py output.fd program.BIN [file2.BIN ...]
```

Le boot loader MO5 (`BOOTMO.BIN`) est **embarqué en dur** dans le script sous
forme de tableau de bytes, ce qui supprime toute dépendance externe.

---

## Format disquette Thomson MO5 (.fd)

### Géométrie

| Constante     | Valeur              | Détail                          |
|---------------|---------------------|---------------------------------|
| `SECTOR_SIZE` | 256 octets          | 1 secteur = 256 octets physiques|
| `SECTOR_BYTES`| 255 octets          | Octets utiles par secteur       |
| `TRACK_SIZE`  | 16 × 256 = 4 096    | 16 secteurs par piste           |
| `DISK_SIZE`   | 80 × 4 096 = 327 680| 80 pistes                       |
| `BLOCK_SIZE`  | 8 × 256 = 2 048     | 8 secteurs par bloc             |
| `BLOCK_BYTES` | 8 × 255 = 2 040     | Octets utiles par bloc          |

### Valeurs spéciales de la FAT

| Constante       | Valeur | Signification                          |
|-----------------|--------|----------------------------------------|
| `FREE_BLOCK`    | `0xFF` | Bloc libre                             |
| `RESERVED_BLOCK`| `0xFE` | Bloc réservé (non utilisable)          |
| `0xC1`–`0xC8`  | —      | Dernier bloc d'un fichier : `0xC0 + nb_secteurs_occupés` |
| `0x00`–`0x4B`  | —      | Bloc appartenant à un fichier : numéro du bloc suivant |

### Zones clés en mémoire

```
Piste 20, secteur 1  → offset 0x14000 : Nom disquette (8 octets) + secteur de boot
Piste 20, secteur 2  → FAT_OFFSET = 20 × TRACK_SIZE + SECTOR_SIZE + 1 = 0x14201
Piste 20, secteur 3  → DIR_OFFSET = 20 × TRACK_SIZE + 2 × SECTOR_SIZE = 0x14400
```

**Note critique :** Le 1er octet du secteur FAT (offset `FAT_OFFSET - 1`) n'est pas utilisé et doit être mis à `0x00`.

---

## BOOTMO.BIN embarqué

Le tableau suivant est le binaire exact compilé depuis `BootMO.asm`
(Thomson MO5, org `$2200`, 98 octets total avec header/footer Thomson).

```python
BOOTMO_BIN = bytes([
    0x00, 0x00, 0x58, 0x22, 0x00,  # header: type=0, size=0x0058, addr=$2200
    0x10, 0xce, 0x20, 0xcc,
    0x86, 0x20, 0x1f, 0x8b,
    0x86, 0x00, 0x97, 0x49,
    0xcc, 0x14, 0x01,
    0x8e, 0x23, 0x00,
    0xbd, 0x22, 0x47,
    0x1f, 0x12,
    0x31, 0x2d,
    0xbe, 0x23, 0x0c,
    0x30, 0x1b,
    0x31, 0x21,
    0xec, 0xa1,
    0x81, 0xff,
    0x27, 0x0e,
    0xbd, 0x22, 0x47,
    0x30, 0x89, 0x00, 0xff,
    0x5c,
    0xe1, 0xa4,
    0x2e, 0xec,
    0x20, 0xf2,
    0x31, 0x3f,
    0xae, 0xa1,
    0x34, 0x20,
    0xad, 0x84,
    0x35, 0x20,
    0xae, 0xa0,
    0x8c, 0xff, 0xff,
    0x26, 0xd7,
    0x3f, 0x28,
    0x34, 0x06,
    0x97, 0x4b,
    0xd7, 0x4c,
    0x9f, 0x4f,
    0x86, 0x02,
    0x97, 0x48,
    0x3f, 0x26,
    0x35, 0x06,
    0x39,
    0xff, 0x00, 0x00, 0x00, 0x00,  # footer Thomson
])
```

**Validations à effectuer avant usage :**
- `len(BOOTMO_BIN) <= 130` (max 120 octets utiles)
- `BOOTMO_BIN[0] == 0x00`
- `BOOTMO_BIN[3] in (0x62, 0x22)` (adresse de boot = `$6200` ou `$2200`)
- `BOOTMO_BIN[4] == 0x00`

---

## Format d'un fichier .BIN Thomson

Tout fichier `.BIN` valide a la structure suivante :

```
[0]    0x00          type (toujours 0x00 pour binaire)
[1-2]  size (BE)     taille du contenu (sans header ni footer)
[3-4]  addr_load (BE) adresse de chargement
--- contenu (size octets) ---
[-5]   0xFF          marqueur de fin
[-4]   0x00
[-3]   0x00
[-2,-1] addr_exec (BE) adresse d'exécution
```

Un fichier `.BIN` produit par CMOC avec `--thommo` contient déjà ce header/footer.

---

## Algorithme complet

### 1. `format_disk(disk, diskname)`

Initialise le tableau de 327 680 octets :

1. Remplir tout le tableau avec `0xE5`
2. Remplir la piste 20 entière (`[20*TRACK_SIZE : 21*TRACK_SIZE]`) avec `0xFF`
3. Écrire le nom de la disquette (stem du fichier de sortie, max 8 chars, padé avec `0x20`) aux 8 premiers octets de la piste 20
4. Mettre `0x00` à `FAT_OFFSET - 1`
5. Mettre `RESERVED_BLOCK` aux blocs `FAT_OFFSET + 2*20` et `FAT_OFFSET + 2*20 + 1`
6. Mettre `0x00` de `FAT_OFFSET + 80*2` jusqu'à `DIR_OFFSET` (exclus)

### 2. `find_free_block(disk, used_blocks) → int`

Trouve un bloc libre en évitant les blocs déjà référencés dans le répertoire.

1. Parcourir le répertoire toutes les 32 entrées à partir de `DIR_OFFSET + 13` (index du premier bloc). Collecter tous les blocs ≠ `FREE_BLOCK` dans une liste `in_use`.
2. Chercher de **79 → 0** : retourner `i` si `disk[FAT_OFFSET + i] == FREE_BLOCK` et `i` n'est pas dans `in_use` ni dans `used_blocks`
3. Sinon chercher de **80 → 159** (même critère)
4. Retourner `FREE_BLOCK` si disquette pleine

### 3. `find_free_entry(disk) → int`

Parcourir le répertoire de 0 à `14 * SECTOR_SIZE` par pas de 32. Retourner l'offset de la première entrée dont l'octet 0 vaut `FREE_BLOCK`.

### 4. `add_file_entry(disk, filename, block, size_left) → int`

Écrit une entrée de répertoire de 32 octets à `DIR_OFFSET + find_free_entry()`.

Structure des 32 octets :

| Octets | Contenu                                                   |
|--------|-----------------------------------------------------------|
| 0–7    | Nom du fichier (sans extension), padé à droite avec `0x20` |
| 8–10   | Extension (sans le `.`), padée à droite avec `0x20`       |
| 11     | Type : `2` si extension `.BIN`/`.CHG`/`.MAP`, sinon `0`   |
| 12     | `0x00` (données binaires)                                 |
| 13     | Numéro du premier bloc                                    |
| 14–15  | `size_left` en big-endian (octets dans le dernier secteur)|
| 16     | `0x00`                                                    |
| 17–23  | `0x20` (commentaire vide)                                 |
| 24     | Jour (1–31)                                               |
| 25     | Mois (1–12)                                               |
| 26     | Année – 2000                                              |
| 27–31  | `0x00`                                                    |

Retourner l'offset absolu de l'entrée dans `disk`.

**Note :** `filename` ici est le **basename uniquement** (`os.path.basename(filepath)`), pas le chemin complet.

### 5. `write_block(disk, block, file_bytes, file_size, offset)`

Écrit jusqu'à 8 secteurs dans un bloc physique :

```
pour b de 0 à 7 :
    src = offset + b * SECTOR_BYTES
    dst = block * BLOCK_SIZE + b * SECTOR_SIZE
    nb  = min(SECTOR_BYTES, file_size - src)  # 0 si src >= file_size
    si nb > 0 :
        disk[dst : dst+nb] = file_bytes[src : src+nb]
        disk[dst+nb : dst+SECTOR_SIZE] = 0x00 (padding)
```

### 6. `add_file_content(disk, filename, file_bytes)`

Alloue des blocs FAT et écrit le contenu complet d'un fichier.

```
blocks = []
offset = 0
size_left = len(file_bytes)
size = len(file_bytes)

tant que size_left > 0 :
    block = find_free_block(disk, blocks)
    write_block(disk, block, file_bytes, size, offset)
    offset += BLOCK_BYTES

    si blocks non vide :
        disk[FAT_OFFSET + blocks[-1]] = block  # chaîne le bloc précédent

    blocks.append(block)

    si size_left < BLOCK_BYTES :
        # dernier bloc : compter les secteurs occupés
        nb_sectors = 0xC0
        remaining = size_left
        tant que remaining > 0 :
            nb_sectors += 1
            remaining -= SECTOR_BYTES
        disk[FAT_OFFSET + blocks[-1]] = nb_sectors

        # size_left dans le dernier secteur = size_left mod SECTOR_BYTES
        # si == 0, c'est SECTOR_BYTES (secteur plein)
        last_sector_bytes = size_left % SECTOR_BYTES
        si last_sector_bytes == 0 : last_sector_bytes = SECTOR_BYTES

        add_file_entry(disk, filename, blocks[0], last_sector_bytes)

    size_left -= BLOCK_BYTES
```

### 7. `add_file(disk, filepath)`

Point d'entrée pour ajouter un fichier depuis le disque :

1. `filename = os.path.basename(filepath)`
2. Lire le fichier en binaire → `file_bytes`
3. Appeler `add_file_content(disk, filename, file_bytes)`

Pas de génération automatique de header/footer : CMOC produit des `.BIN` déjà valides.

### 8. `add_boot_loader(disk, nb_files)`

C'est la partie la plus critique. Elle écrit le boot loader dans le secteur physique 0 du fichier `.fd` et les descripteurs de chargement dans la piste 20.

#### 8a. Écriture du boot sector (octets 0–255 du fichier .fd)

```
Mettre disk[0:256] à 0x00

checksum = 0x55
content_size = len(BOOTMO_BIN) - 10  # sans header (5) ni footer (5)

pour i de 0 à content_size - 1 :
    disk[i] = (256 - BOOTMO_BIN[i + 5]) & 0xFF
    checksum = (checksum + BOOTMO_BIN[i + 5]) & 0xFF

Écrire b'BASIC2' aux octets disk[120:126]
checksum = (checksum + 0x6C) & 0xFF
disk[127] = checksum

disk[FAT_OFFSET] = RESERVED_BLOCK  # bloc 0 réservé
```

#### 8b. Descripteurs de fichiers dans la piste 20

Position d'écriture : `n = 12`, base = `20 * TRACK_SIZE`

Pour chaque fichier `f_idx` de `0` à `nb_files - 1` :

```
entry = f_idx * 32
block = disk[DIR_OFFSET + entry + 13]  # premier bloc du fichier

# Lire le header Thomson depuis le début du bloc physique
file_size = (disk[block*BLOCK_SIZE + 1] << 8) | disk[block*BLOCK_SIZE + 2]
file_addr = (disk[block*BLOCK_SIZE + 3] << 8) | disk[block*BLOCK_SIZE + 4]

# Écrire l'adresse de chargement (2 octets)
disk[base + n] = file_addr >> 8;   n += 1
disk[base + n] = file_addr & 0xFF; n += 1

# Parcourir les blocs du fichier pour écrire piste/secteurs
file_exec = 0
current_block = block

tant que current_block != FREE_BLOCK :
    next_b = disk[FAT_OFFSET + current_block]
    nbs = 8
    si next_b > 0xC0 :
        nbs = next_b - 0xC0
        next_b = FREE_BLOCK
        # Récupérer l'adresse d'exécution dans le footer Thomson
        nb_bytes = (disk[DIR_OFFSET + entry + 14] << 8) | disk[DIR_OFFSET + entry + 15]
        src = current_block * BLOCK_SIZE + (nbs - 1) * SECTOR_SIZE + nb_bytes
        si nb_bytes < 2 :
            file_exec = disk[src - 3]
        sinon :
            file_exec = disk[src - 2]
        file_exec = (file_exec << 8) | disk[src - 1]

    track  = current_block >> 1
    sector = 9 si (current_block & 1) sinon 1

    disk[base + n] = track;             n += 1
    disk[base + n] = sector;            n += 1
    disk[base + n] = sector + nbs - 1;  n += 1

    current_block = next_b

# Marqueur de fin + adresse d'exécution
disk[base + n] = FREE_BLOCK;              n += 1
disk[base + n] = (file_exec >> 8) & 0xFF; n += 1
disk[base + n] = file_exec & 0xFF;         n += 1
```

### 9. `main()`

```
argv[1] = output.fd
argv[2:] = liste de fichiers .BIN à embarquer

disk = bytearray(DISK_SIZE)
format_disk(disk, argv[1])

pour chaque filepath dans argv[2:] :
    add_file(disk, filepath)

add_boot_loader(disk, len(argv[2:]))

écrire disk dans argv[1]
```

---

## Intégration dans le Makefile

Remplacer dans le Makefile :

```makefile
# Avant
FDFS       := $(BOOTFD_DIR)/tools/fdfs
BOOTMO_BIN := $(BOOTFD_DIR)/BOOTMO.BIN

$(DISK_IMAGE): $(PROGRAM_BIN)
    @$(FDFS) -addBL $@ $(BOOTMO_BIN) $(PROGRAM_BIN)
```

```makefile
# Après
MAKEFD := python3 $(TOOLS_DIR)/scripts/makefd.py

$(DISK_IMAGE): $(PROGRAM_BIN)
    @$(MAKEFD) $@ $(PROGRAM_BIN)
```

Supprimer les targets `install-bootfd` et les variables `BOOTFD_DIR`, `FDFS`, `BOOTMO_BIN`.

---

## Points d'attention critiques

1. **`filename` dans le répertoire** : toujours utiliser `os.path.basename()`, jamais le chemin complet (sinon le nom est corrompu).
2. **`FAT_OFFSET`** vaut `20*TRACK_SIZE + SECTOR_SIZE + 1` (le +1 est intentionnel : le 1er octet du secteur n'est pas utilisé, la FAT commence au 2e octet).
3. **`size_left` dans `add_file_entry`** : c'est le nombre d'octets dans le **dernier secteur**, pas la taille totale. Calculer `size % SECTOR_BYTES`, et si le résultat est 0, utiliser `SECTOR_BYTES`.
4. **Récupération de `file_exec`** : l'adresse d'exécution est dans le footer Thomson (`0xFF 0x00 0x00 addr_exec`). Elle se trouve à la fin du dernier secteur du dernier bloc. Le calcul de `src` dépend de `nb_bytes` (octets dans le dernier secteur de l'entrée répertoire).
5. **Ordre des opérations** : les fichiers doivent être ajoutés avec `add_file_content` **avant** d'appeler `add_boot_loader`, car cette dernière lit le répertoire et la FAT déjà remplis.
6. **Bloc 0 réservé** : après écriture du boot sector, `disk[FAT_OFFSET] = RESERVED_BLOCK`.
