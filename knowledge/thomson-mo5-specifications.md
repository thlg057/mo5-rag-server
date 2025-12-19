# 🖥️ Thomson MO5 - Technical Specifications

## 📋 General characteristics

### **Processor**
- **CPU** : Motorola 6809 at 1 MHz
- **Architecture** : 8-bit with 16-bit registers
- **Memory** : 48 KB RAM + 16 KB ROM

### **Display**
- **Resolutions** : 
  - Text mode : 40x25 characters
  - Graphics mode : 320x200 pixels
- **Colors** : 16 simultaneous colors
- **Video memory** : Separate color/form planes

## 🎮 System calls

### **System calls (SWI)**
```asm
; Display a character
swi
fcb $02

; Read a character
swi  
fcb $0A
```

### **Important registers**
- **$A7C0** : Video control (bit 0 : 0=color, 1=form)
- **$A7CB** : MO5 memory paging
- **$A7E6** : MO6 memory paging

## 🖼️ Video memory organization

### **Graphics mode 320x200**
- **Width** : 40 bytes per line
- **Pixels** : 2 pixels per byte
- **Address calculation** : `Y * 40 + (X / 2)`
- **Total size** : 8000 bytes (320x200/2)

### **Video modes**
```asm
; Select color mode
lda $A7C0
anda #%11111110
sta $A7C0

; Select form mode
lda $A7C0
ora #%00000001
sta $A7C0
```

## 💾 Memory organization

### **Important memory areas**
- **$0000-$1FFF** : Video memory
- **$2000-$9FFF** : User RAM
- **$A000-$BFFF** : System ROM
- **$C000-$EFFF** : BASIC ROM
- **$F000-$FFFF** : Monitor ROM

### **Recommended working area**
- **$2600-$9FFF** : Safe area for programs
- **$2100-$26FF** : Temporary buffer (1536 bytes)

## 🎯 Important constraints

### **System limitations**
1. **No system stack** : Use `$20CC` as stack address
2. **Limited system calls** : Only SWI with FCB codes
3. **No printf/scanf** : Use native system calls
4. **Limited memory** : Optimize memory usage

### **Interrupt handling**
- **IRQ** : Hardware interrupts
- **FIRQ** : Fast interrupts
- **NMI** : Non-maskable interrupt
- **SWI** : Software interrupt (system calls)

## 🔧 Development tools

### **Assembler**
- **c6809** : 6809 assembler
- **lwasm** : Alternative assembler

### **C compiler**
- **CMOC** : C compiler for 6809
- **Options** : `--thommo --org=2600`

### **Disk image creation**
- **fdfs** : Creation of .fd images for Thomson

## 📏 Coding conventions

### **Assembly**
```asm
; Comments with semicolon
 org $2600          ; Start address
 
LABEL               ; Labels without indentation
 instruction        ; Instructions with indentation
 
 end MAIN           ; Entry point
```

### **C with CMOC**
```c
// No standard libraries
// Use Thomson system calls

void function(void) {
    // Standard C code
    asm {
        // Embedded assembly code
    }
}
```

## ⚠️ Pitfalls to avoid

### **Common errors**
1. **Using printf()** : Doesn't exist on Thomson MO5
2. **Forgetting headers** : .BIN files must have a header
3. **Wrong address** : Load at $2600 minimum
4. **Uninitialized stack** : Set stack to $20CC
5. **Incorrect video mode** : Properly alternate color/form

### **Recommended optimizations**
1. **Reuse registers** : Save memory
2. **Avoid divisions** : Use shifts
3. **Group memory accesses** : Reduce access times
4. **Use addressing modes** : Optimize code

## 🎮 Standard controls

### **Thomson MO5 game keys**
- **Arrows** : $08, $09, $0A, $0B
- **Space** : $20
- **Letters** : $41-$5A (A-Z)
- **Numbers** : $30-$39 (0-9)

### **Control codes**
- **$0C** : Form Feed (clear screen)
- **$0D** : Carriage Return
- **$0A** : Line Feed

This technical documentation gives you all the basics needed to develop efficiently on Thomson MO5.
