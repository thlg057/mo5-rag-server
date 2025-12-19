# 🔧 Thomson MO5 Compilation Tools

## 📋 Required tools

### **For assembly**
- **c6809** : Main assembler for 6809
- **lwasm** : Alternative assembler (lwtools)

### **For C**
- **CMOC** : C compiler for 6809

### **For disk images**
- **fdfs** : Creation of .fd images for Thomson

## 🛠️ Tools installation

### **Ubuntu/Debian**
```bash
# Install basic tools
sudo apt update
sudo apt install build-essential

# Install lwtools (contains lwasm)
sudo apt install lwtools

# CMOC must be compiled from sources
# See: https://github.com/stahta01/cmoc
```

### **CMOC compilation**
```bash
# Clone repository
git clone https://github.com/stahta01/cmoc.git
cd cmoc

# Compile and install
./configure --prefix=/usr/local
make
sudo make install

# Verify installation
cmoc --version
```

### **Thomson-specific tools**
```bash
# c6809 and fdfs are often provided with Thomson distributions
# Or available on Thomson retro-computing sites
```

## 📁 Recommended project structure

```
my_project/
├── src/                    # Source code
│   ├── main.asm           # Main assembly program
│   ├── main.c             # Main C program
│   └── common.h           # Common definitions (C)
├── bin/                   # Compiled files (.BIN)
├── output/                # Disk images (.fd)
├── tools/                 # Compilation tools
│   ├── c6809              # Assembler
│   ├── fdfs               # Image creator
│   └── BOOTMO.BIN         # Thomson MO5 bootloader
└── Makefile               # Compilation automation
```

## 🔨 Example Makefile

### **Complete Makefile**
```makefile
# Variables
C6809 ?= tools/c6809
FDFS  ?= tools/fdfs
CMOC  ?= cmoc

BIN_DIR = bin
OUTPUT_DIR = output
SRC_DIR = src

# Main targets
.PHONY: all clean asm c

all: asm c

asm: $(BIN_DIR)/PROGRAM_ASM.BIN $(OUTPUT_DIR)/program_asm.fd

c: $(BIN_DIR)/PROGRAM_C.BIN $(OUTPUT_DIR)/program_c.fd

# Assembly compilation
$(BIN_DIR)/PROGRAM_ASM.BIN: $(SRC_DIR)/main.asm | bin-dir
	@echo "[ASM] $< -> $@"
	"$(C6809)" -bl $< $@

# C compilation
$(BIN_DIR)/PROGRAM_C.BIN: $(SRC_DIR)/main.c | bin-dir
	@echo "[CMOC] $< -> $@"
	"$(CMOC)" --thommo --org=2600 -o $@ $<

# Disk image creation
$(OUTPUT_DIR)/program_asm.fd: $(BIN_DIR)/PROGRAM_ASM.BIN | output-dir
	@echo "[FDFS] $< -> $@"
	"$(FDFS)" -addBL $@ tools/BOOTMO.BIN $<

$(OUTPUT_DIR)/program_c.fd: $(BIN_DIR)/PROGRAM_C.BIN | output-dir
	@echo "[FDFS] $< -> $@"
	"$(FDFS)" -addBL $@ tools/BOOTMO.BIN $<

# Directory creation
bin-dir:
	@mkdir -p $(BIN_DIR)

output-dir:
	@mkdir -p $(OUTPUT_DIR)

# Cleanup
clean:
	rm -rf $(BIN_DIR) $(OUTPUT_DIR)
	rm -f $(SRC_DIR)/*.lst $(SRC_DIR)/*.html

# Help
help:
	@echo "Available targets:"
	@echo "  all     - Compile everything (assembly + C)"
	@echo "  asm     - Compile only assembly"
	@echo "  c       - Compile only C"
	@echo "  clean   - Clean generated files"
	@echo "  help    - Show this help"
```

## 🎯 Compilation commands

### **Assembly with c6809**
```bash
# Simple compilation
c6809 -bl program.asm program.BIN

# Useful options
c6809 -bl -l program.asm program.BIN  # With listing
c6809 -bl -s program.asm program.BIN  # With symbols
```

### **C with CMOC**
```bash
# Compilation for Thomson MO5
cmoc --thommo --org=2600 -o program.BIN program.c

# Important options
--thommo        # Target Thomson MO5/MO6
--org=2600      # Load address
-o file.BIN     # Output file
-O              # Optimization
-v              # Verbose mode
```

### **Disk image creation**
```bash
# Create .fd image
fdfs -addBL image.fd BOOTMO.BIN PROGRAM.BIN

# fdfs options
-addBL          # Add with bootloader
-list           # List contents
-extract        # Extract file
```

## ⚙️ Advanced configuration

### **Environment variables**
```bash
# In .bashrc or .profile
export CMOC_PATH="/usr/local/bin/cmoc"
export C6809_PATH="/opt/thomson/c6809"
export FDFS_PATH="/opt/thomson/fdfs"
```

### **Automatic compilation script**
```bash
#!/bin/bash
# compile.sh - Automatic compilation script

set -e  # Stop on error

echo "=== Thomson MO5 Compilation ==="

# Tool verification
if ! command -v cmoc &> /dev/null; then
    echo "ERROR: CMOC not found"
    exit 1
fi

# Compilation
echo "Compiling C program..."
cmoc --thommo --org=2600 -o bin/PROGRAM.BIN src/main.c

echo "Creating disk image..."
fdfs -addBL output/program.fd tools/BOOTMO.BIN bin/PROGRAM.BIN

echo "✅ Compilation completed!"
echo "Image created: output/program.fd"
```

## 🔍 Debugging and optimization

### **CMOC debugging options**
```bash
# Compilation with debug information
cmoc --thommo --org=2600 -g -o program.BIN program.c

# Generate assembly listing
cmoc --thommo --org=2600 -S -o program.s program.c
```

### **Optimization**
```bash
# Optimization level 1
cmoc --thommo --org=2600 -O -o program.BIN program.c

# Optimization level 2 (more aggressive)
cmoc --thommo --org=2600 -O2 -o program.BIN program.c
```

### **Size analysis**
```bash
# Compiled file size
ls -la bin/PROGRAM.BIN

# Symbol analysis (with c6809)
c6809 -bl -s program.asm program.BIN
cat program.lst  # View listing
```

## ⚠️ Common problems

### **"command not found" error**
```bash
# Check installation
which cmoc
which c6809
which fdfs

# Add to PATH if necessary
export PATH=$PATH:/path/to/tools
```

### **CMOC compilation error**
```bash
# Check C syntax
cmoc --thommo --check-only program.c

# Verbose compilation for more info
cmoc --thommo --org=2600 -v -o program.BIN program.c
```

### **fdfs error**
```bash
# Check that BOOTMO.BIN exists
ls -la tools/BOOTMO.BIN

# Create image with force
fdfs -force -addBL image.fd tools/BOOTMO.BIN program.BIN
```

## 📊 Tool comparison

| Tool | Advantages | Disadvantages |
|------|------------|---------------|
| **c6809** | Native assembler, complete control | Specific syntax |
| **lwasm** | Modern syntax, portable | Less Thomson-specialized |
| **CMOC** | Standard C, ease of use | Larger code size |
| **fdfs** | Native Thomson format | Command-line interface |

## 🚀 Complete automation

### **Development script**
```bash
#!/bin/bash
# dev.sh - Automated development

# Automatic compilation on file modification
while inotifywait -e modify src/; do
    echo "File modified, recompiling..."
    make clean && make
    echo "✅ Ready for testing in DCMOTO"
done
```

This configuration gives you a complete and efficient development environment for Thomson MO5.
