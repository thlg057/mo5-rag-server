# 💾 Thomson MO5 Disk Image Creation

## 📋 Overview

To create disk images (.fd) compatible with Thomson MO5, we use **BootFloppyDisk**, a specialized project that provides:
- **fdfs** : Disk image creation tool
- **BOOTMO.BIN** : Thomson MO5 bootloader
- **Scripts** : Image creation automation

## 🛠️ BootFloppyDisk installation

### **Automatic method (recommended)**
```bash
#!/bin/bash
# install.sh - Automatic BootFloppyDisk installation

set -e

echo "=== BootFloppyDisk Installation for Thomson MO5 ==="

# Configuration
REPO_URL="https://github.com/OlivierP-To8/BootFloppyDisk.git"
INSTALL_DIR="tools/BootFloppyDisk"

# Git verification
if ! command -v git &> /dev/null; then
    echo "ERROR: git is not installed"
    echo "Install git with: sudo apt install git"
    exit 1
fi

# Repository cloning
echo "Cloning BootFloppyDisk..."
if [ -d "$INSTALL_DIR" ]; then
    echo "Existing directory, updating..."
    cd "$INSTALL_DIR"
    git pull
    cd ../..
else
    git clone "$REPO_URL" "$INSTALL_DIR"
fi

# Tool compilation
echo "Compiling tools..."
cd "$INSTALL_DIR/tools"
make clean
make

# Installation verification
if [ -f "fdfs" ]; then
    echo "✅ fdfs compiled successfully"
else
    echo "❌ fdfs compilation error"
    exit 1
fi

# Return to main directory
cd ../../..

echo "✅ Installation completed!"
echo "Tools available in: $INSTALL_DIR/tools/"
echo "  - fdfs: Disk image creation"
echo "  - BOOTMO.BIN: Thomson MO5 bootloader"
```

### **Manual installation**
```bash
# Clone repository
git clone https://github.com/OlivierP-To8/BootFloppyDisk.git tools/BootFloppyDisk

# Compile tools
cd tools/BootFloppyDisk/tools
make

# Verify installation
ls -la fdfs BOOTMO.BIN
```

## 🔧 Using fdfs

### **Basic commands**
```bash
# Create empty disk image
fdfs -new image.fd

# Add program with bootloader
fdfs -addBL image.fd BOOTMO.BIN PROGRAM.BIN

# Add simple file
fdfs -add image.fd FILE.BIN

# List image contents
fdfs -list image.fd

# Extract file
fdfs -extract image.fd FILE.BIN extracted_file.bin
```

### **Important options**
```bash
-new            # Create new empty image
-addBL          # Add with bootloader (recommended)
-add            # Add simple file
-list           # List contents
-extract        # Extract file
-force          # Force overwrite
-verbose        # Verbose mode
```

## 🎯 Creating images for your programs

### **Assembly program**
```bash
# 1. Compile assembly program
c6809 -bl program.asm program.BIN

# 2. Create disk image
tools/BootFloppyDisk/tools/fdfs -addBL output/program.fd \
    tools/BootFloppyDisk/tools/BOOTMO.BIN \
    program.BIN

# 3. Verify contents
tools/BootFloppyDisk/tools/fdfs -list output/program.fd
```

### **C program**
```bash
# 1. Compile C program
cmoc --thommo --org=2600 -o program.BIN program.c

# 2. Create disk image
tools/BootFloppyDisk/tools/fdfs -addBL output/program.fd \
    tools/BootFloppyDisk/tools/BOOTMO.BIN \
    program.BIN

# 3. Test in DCMOTO
# Load output/program.fd in emulator
```

### **Multiple programs on one disk**
```bash
# Create empty image
fdfs -new output/collection.fd

# Add bootloader
fdfs -add output/collection.fd tools/BootFloppyDisk/tools/BOOTMO.BIN

# Add multiple programs
fdfs -add output/collection.fd program1.BIN
fdfs -add output/collection.fd program2.BIN
fdfs -add output/collection.fd program3.BIN

# List contents
fdfs -list output/collection.fd
```

## 📁 Recommended project structure

```
my_project/
├── src/                           # Source code
│   ├── main.asm
│   └── main.c
├── bin/                           # Compiled programs
│   ├── MAIN_ASM.BIN
│   └── MAIN_C.BIN
├── output/                        # Disk images
│   ├── main_asm.fd
│   └── main_c.fd
├── tools/
│   └── BootFloppyDisk/           # Image creation tools
│       ├── tools/
│       │   ├── fdfs              # Creation tool
│       │   └── BOOTMO.BIN        # MO5 bootloader
│       └── README.md
├── install.sh                    # Installation script
└── Makefile                      # Automation
```

## 🔨 Automation with Makefile

### **Complete Makefile with image creation**
```makefile
# Variables
FDFS = tools/BootFloppyDisk/tools/fdfs
BOOTMO = tools/BootFloppyDisk/tools/BOOTMO.BIN
C6809 = tools/c6809
CMOC = cmoc

BIN_DIR = bin
OUTPUT_DIR = output
SRC_DIR = src

# Tool verification
.PHONY: check-tools
check-tools:
	@if [ ! -f "$(FDFS)" ]; then \
		echo "ERROR: fdfs not found. Run ./install.sh"; \
		exit 1; \
	fi
	@if [ ! -f "$(BOOTMO)" ]; then \
		echo "ERROR: BOOTMO.BIN not found. Run ./install.sh"; \
		exit 1; \
	fi

# Directory creation
bin-dir:
	@mkdir -p $(BIN_DIR)

output-dir:
	@mkdir -p $(OUTPUT_DIR)

# Assembly compilation
$(BIN_DIR)/%.BIN: $(SRC_DIR)/%.asm | bin-dir
	@echo "[ASM] $< -> $@"
	"$(C6809)" -bl $< $@

# C compilation
$(BIN_DIR)/%.BIN: $(SRC_DIR)/%.c | bin-dir
	@echo "[CMOC] $< -> $@"
	"$(CMOC)" --thommo --org=2600 -o $@ $<

# Disk image creation
$(OUTPUT_DIR)/%.fd: $(BIN_DIR)/%.BIN | output-dir check-tools
	@echo "[FDFS] $< -> $@"
	"$(FDFS)" -addBL $@ "$(BOOTMO)" $<

# Main targets
all: $(OUTPUT_DIR)/main_asm.fd $(OUTPUT_DIR)/main_c.fd

# Cleanup
clean:
	rm -rf $(BIN_DIR) $(OUTPUT_DIR)

# Tool installation
install:
	./install.sh

# Test created images
test:
	@echo "Created disk images:"
	@ls -la $(OUTPUT_DIR)/*.fd
	@echo ""
	@echo "Image contents:"
	@for fd in $(OUTPUT_DIR)/*.fd; do \
		echo "=== $$fd ==="; \
		"$(FDFS)" -list $$fd; \
		echo ""; \
	done

# Help
help:
	@echo "Available targets:"
	@echo "  all      - Compile and create all images"
	@echo "  install  - Install BootFloppyDisk"
	@echo "  test     - Show created images"
	@echo "  clean    - Clean generated files"
	@echo "  help     - Show this help"
```

## 🎮 Testing in DCMOTO

### **Loading an image**
1. **Launch DCMOTO**
2. **File Menu** → **Insert disk**
3. **Select** your .fd file
4. **Restart** emulator (Ctrl+R)
5. **Program launches automatically**

### **Content verification**
```bash
# List image contents
tools/BootFloppyDisk/tools/fdfs -list output/program.fd

# Example output:
# BOOTMO.BIN    256 bytes
# PROGRAM.BIN   1024 bytes
```

## 🔍 Image debugging

### **Common problems**
```bash
# Error: "BOOTMO.BIN not found"
# ✅ Solution: Check path
ls -la tools/BootFloppyDisk/tools/BOOTMO.BIN

# Error: "Invalid BIN file"
# ✅ Solution: Check compilation
file bin/PROGRAM.BIN

# Error: "Disk image full"
# ✅ Solution: Create new image
fdfs -new output/new.fd
```

### **Image validation**
```bash
# Validation script
#!/bin/bash
validate_image() {
    local image=$1
    echo "Validating $image..."
    
    if [ ! -f "$image" ]; then
        echo "❌ File doesn't exist"
        return 1
    fi
    
    # List contents
    echo "Contents:"
    fdfs -list "$image"
    
    # Check size
    local size=$(stat -c%s "$image")
    echo "Size: $size bytes"
    
    if [ $size -gt 0 ]; then
        echo "✅ Valid image"
        return 0
    else
        echo "❌ Empty image"
        return 1
    fi
}

# Usage
validate_image "output/program.fd"
```

## 📊 Thomson disk formats

### **Technical specifications**
- **Format** : .fd (DCMOTO format)
- **Size** : 80 tracks, 1 side, 16 sectors/track
- **Capacity** : ~160 KB per disk
- **Sector** : 256 bytes
- **Bootloader** : BOOTMO.BIN (256 bytes)

### **Compatibility**
- ✅ **DCMOTO** : Native format
- ✅ **Thomson MO5** : Compatible with real drive
- ✅ **Thomson MO6** : Compatible
- ✅ **Other emulators** : Generally supported

## 🚀 Useful scripts

### **Quick creation script**
```bash
#!/bin/bash
# create_disk.sh - Quick disk image creation

if [ $# -ne 2 ]; then
    echo "Usage: $0 <program.BIN> <image.fd>"
    exit 1
fi

PROGRAM=$1
IMAGE=$2
FDFS="tools/BootFloppyDisk/tools/fdfs"
BOOTMO="tools/BootFloppyDisk/tools/BOOTMO.BIN"

echo "Creating $IMAGE with $PROGRAM..."

# Create image
"$FDFS" -addBL "$IMAGE" "$BOOTMO" "$PROGRAM"

# Verify
echo "Image contents:"
"$FDFS" -list "$IMAGE"

echo "✅ Image created: $IMAGE"
echo "Ready for DCMOTO!"
```

This documentation gives you everything needed to efficiently create and manage your Thomson MO5 disk images!
