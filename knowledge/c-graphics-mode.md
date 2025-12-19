# 🎨 Thomson MO5 C - Graphics Mode

## 📋 Basic concepts

### **Thomson MO5 video system**
Thomson MO5 uses a **dual-plane** system:
- **Color plane** : Pixel colors
- **Form plane** : Pixels on/off

### **Basic functions**
```c
// Select color plane
void videoc_a(void) {
    asm {
        lda $A7C0
        anda #%11111110
        sta $A7C0
    }
}

// Select form plane
void videof(void) {
    asm {
        lda $A7C0
        ora #%00000001
        sta $A7C0
    }
}

// Screen address calculation (320x200, 40 bytes/line)
unsigned int calc_screen_addr(unsigned char x, unsigned char y) {
    return (unsigned int)y * 40 + (x >> 1);
}
```

## 🖼️ Sprite management

### **Sprite structure**
```c
// Global variables for sprite
unsigned char object_x = 160;  // X position (center)
unsigned char object_y = 100;  // Y position (center)
unsigned char old_x = 160;     // Old X position
unsigned char old_y = 100;     // Old Y position

// Buffer to save background (32 bytes for 16x16 sprite)
unsigned char buffer_fond[32];
```

### **Background saving**
```c
// Save background of a 16x16 sprite
void save_background(unsigned int screen_addr) {
    unsigned char i;
    unsigned char *screen = (unsigned char *)screen_addr;
    unsigned char *buf = buffer_fond;
    
    // Save color
    videoc_a();
    for (i = 0; i < 16; i++) {
        *buf++ = screen[0];
        *buf++ = screen[1];
        screen += 40;  // Next line
    }
    
    // Save form
    videof();
    screen = (unsigned char *)screen_addr;
    for (i = 0; i < 16; i++) {
        *buf++ = screen[0];
        *buf++ = screen[1];
        screen += 40;  // Next line
    }
}
```

### **Background restoration**
```c
// Restore background of a 16x16 sprite
void restore_background(unsigned int screen_addr) {
    unsigned char i;
    unsigned char *screen = (unsigned char *)screen_addr;
    unsigned char *buf = buffer_fond;
    
    // Restore color
    videoc_a();
    for (i = 0; i < 16; i++) {
        screen[0] = *buf++;
        screen[1] = *buf++;
        screen += 40;  // Next line
    }
    
    // Restore form
    videof();
    screen = (unsigned char *)screen_addr;
    for (i = 0; i < 16; i++) {
        screen[0] = *buf++;
        screen[1] = *buf++;
        screen += 40;  // Next line
    }
}
```

## 🎨 Sprite display

### **Simple sprite (white square)**
```c
// Display a simple 16x16 sprite
void draw_sprite(unsigned int screen_addr) {
    unsigned char i;
    unsigned char *screen = (unsigned char *)screen_addr;
    
    // Display color (white on white background)
    videoc_a();
    for (i = 0; i < 16; i++) {
        screen[0] = 0x77;  // White on white background
        screen[1] = 0x77;
        screen += 40;  // Next line
    }
    
    // Display form (full square)
    videof();
    screen = (unsigned char *)screen_addr;
    for (i = 0; i < 16; i++) {
        screen[0] = 0xFF;  // Full pixels
        screen[1] = 0xFF;
        screen += 40;  // Next line
    }
}
```

### **Custom colored sprite**
```c
// Color data for sprite (16 lines x 2 bytes)
unsigned char sprite_colors[32] = {
    0x11, 0x11,  // Red on red background
    0x22, 0x22,  // Green on green background
    0x33, 0x33,  // Yellow on yellow background
    0x44, 0x44,  // Blue on blue background
    0x55, 0x55,  // Magenta on magenta background
    0x66, 0x66,  // Cyan on cyan background
    0x77, 0x77,  // White on white background
    0x00, 0x00,  // Black on black background
    0x11, 0x11,  // Red
    0x22, 0x22,  // Green
    0x33, 0x33,  // Yellow
    0x44, 0x44,  // Blue
    0x55, 0x55,  // Magenta
    0x66, 0x66,  // Cyan
    0x77, 0x77,  // White
    0x00, 0x00   // Black
};

// Form data for sprite (16 lines x 2 bytes)
unsigned char sprite_forms[32] = {
    0xFF, 0xFF,  // Full line
    0xC3, 0xC3,  // Line with holes
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0x81, 0x81,  // Border
    0xC3, 0xC3,  // Line with holes
    0xFF, 0xFF,  // Full line
    0x00, 0x00   // Empty line
};

// Colored sprite display
void draw_colored_sprite(unsigned int screen_addr) {
    unsigned char i;
    unsigned char *screen = (unsigned char *)screen_addr;
    unsigned char *colors = sprite_colors;
    unsigned char *forms = sprite_forms;
    
    // Display color
    videoc_a();
    for (i = 0; i < 16; i++) {
        screen[0] = *colors++;
        screen[1] = *colors++;
        screen += 40;
    }
    
    // Display form
    videof();
    screen = (unsigned char *)screen_addr;
    for (i = 0; i < 16; i++) {
        screen[0] = *forms++;
        screen[1] = *forms++;
        screen += 40;
    }
}
```

## 🎮 Smooth movement

### **Movement handling**
```c
// Display object at current position
void show_object(void) {
    unsigned int addr = calc_screen_addr(object_x, object_y);
    save_background(addr);
    draw_sprite(addr);  // or draw_colored_sprite(addr)
}

// Erase object at current position
void hide_object(void) {
    unsigned int addr = calc_screen_addr(old_x, old_y);
    restore_background(addr);
}

// Object movement
void move_object(char direction) {
    // Save old position
    old_x = object_x;
    old_y = object_y;
    
    // Calculate new position with boundaries
    switch (direction) {
        case 'Z':
        case 'z':
            if (object_y >= 8) object_y -= 5;  // Top limit
            break;
        case 'S':
        case 's':
            if (object_y <= 184) object_y += 5;  // Bottom limit (200-16)
            break;
        case 'Q':
        case 'q':
            if (object_x >= 8) object_x -= 5;  // Left limit
            break;
        case 'D':
        case 'd':
            if (object_x <= 304) object_x += 5;  // Right limit (320-16)
            break;
    }
    
    // Update display
    hide_object();
    show_object();
}
```

## 🎯 Complete program

### **Working template**
```c
/*
 * Thomson MO5 C Program - Graphics Mode
 * Uses real Thomson MO5 graphics techniques
 */

// Thomson MO5 system calls
void mo5_putchar(char c) {
    asm {
        ldb c
        swi
        fcb $02
    }
}

char mo5_getchar(void) {
    asm {
        swi
        fcb $0A
        tfr b,a
    }
}

void mo5_clear_screen(void) {
    mo5_putchar(12);
}

// Thomson MO5 graphics functions
void videoc_a(void) {
    asm {
        lda $A7C0
        anda #%11111110
        sta $A7C0
    }
}

void videof(void) {
    asm {
        lda $A7C0
        ora #%00000001
        sta $A7C0
    }
}

// Global variables
unsigned char object_x = 160;
unsigned char object_y = 100;
unsigned char old_x = 160;
unsigned char old_y = 100;
unsigned char buffer_fond[32];

// Screen address calculation
unsigned int calc_screen_addr(unsigned char x, unsigned char y) {
    return (unsigned int)y * 40 + (x >> 1);
}

// Insert here all functions defined above:
// save_background(), restore_background(), draw_sprite(), etc.

// User interface
void show_interface(void) {
    mo5_clear_screen();
    
    asm {
        ldx #msg_title
        jsr print_string
        ldx #msg_controls
        jsr print_string
        ldx #msg_position
        jsr print_string
    }
    
    // Display position (embedded assembly code)
    asm {
        lda object_x
        jsr print_number
    }
    
    mo5_putchar(' ');
    
    asm {
        lda object_y
        jsr print_number
    }
    
    mo5_putchar('\r');
    mo5_putchar('\n');
}

// Main program
int main(void) {
    char key;
    
    // Initialization
    show_interface();
    show_object();
    
    // Game loop
    while (1) {
        key = mo5_getchar();
        
        switch (key) {
            case 'Z':
            case 'z':
            case 'S':
            case 's':
            case 'Q':
            case 'q':
            case 'D':
            case 'd':
                move_object(key);
                break;
            case 'E':
            case 'e':
                goto exit_game;
            case 'R':
            case 'r':
                show_interface();
                show_object();
                break;
        }
    }
    
exit_game:
    hide_object();
    mo5_clear_screen();
    
    asm {
        ldx #msg_exit
        jsr print_string
    }
    
    return 0;
}

// Embedded assembly routines
asm {
print_string:
    lda ,x+
    beq print_string_end
    swi
    fcb $02
    bra print_string
print_string_end:
    rts

print_number:
    cmpa #10
    blo print_single
    pshs a
    ldb #10
    lda #0
count_tens:
    inca
    subb #10
    bpl count_tens
    deca
    adda #$30
    tfr a,b
    swi
    fcb $02
    puls a
    ldb #10
calc_units:
    subb #10
    bpl calc_units
    addb #10
    addb #$30
    swi
    fcb $02
    rts
print_single:
    adda #$30
    tfr a,b
    swi
    fcb $02
    rts

msg_title:
    fcb $0D,$0A
    fcc "=== REAL MO5 GRAPHICS TECHNIQUES ==="
    fcb $0D,$0A,$0D,$0A,0

msg_controls:
    fcc "Z=Up S=Down Q=Left D=Right"
    fcb $0D,$0A
    fcc "E=Quit R=Redraw"
    fcb $0D,$0A,$0D,$0A,0

msg_position:
    fcc "Position: X="
    fcb 0

msg_exit:
    fcc "Graphics test completed!"
    fcb $0D,$0A
    fcc "Techniques based on Evil Dungeons II"
    fcb $0D,$0A,0
}
```

## ⚠️ Errors to avoid

### **Video plane management**
```c
// ❌ INCORRECT - Forgetting to change plane
videoc_a();
draw_sprite_color();
draw_sprite_form();  // Still in color mode!

// ✅ CORRECT - Change plane between color and form
videoc_a();
draw_sprite_color();
videof();
draw_sprite_form();
```

### **Mandatory background saving**
```c
// ❌ INCORRECT - Not saving background
draw_sprite(addr);  // Overwrites background permanently

// ✅ CORRECT - Always save before displaying
save_background(addr);
draw_sprite(addr);
// Later...
restore_background(addr);
```

### **Address calculation**
```c
// ❌ INCORRECT - Wrong calculation
unsigned int addr = x * y;  // Incorrect formula

// ✅ CORRECT - Thomson MO5 formula
unsigned int addr = (unsigned int)y * 40 + (x >> 1);
```

### **Sprite boundaries**
```c
// ❌ INCORRECT - No boundary checking
object_x = 350;  // Exceeds screen width (320)

// ✅ CORRECT - Check boundaries
if (object_x <= 304) object_x += 5;  // 320 - 16 = 304
```

## 🚀 Compilation

### **CMOC command**
```bash
cmoc --thommo --org=2600 -o graphics.BIN graphics.c
```

### **Disk image creation**
```bash
fdfs -addBL graphics.fd BOOTMO.BIN GRAPHICS.BIN
```

## 📊 Advantages of C graphics

### **Compared to assembly**
- **Readability** : Code easier to understand
- **Maintenance** : Simpler modifications
- **Structures** : Easier sprite data management
- **Debugging** : Easier to debug

### **Performance**
- **Smooth** : No flickering with proper management
- **Fast** : Acceptable performance for most games
- **Optimizable** : Embedded assembly for critical parts

This C approach gives you an excellent compromise between development ease and graphics performance on Thomson MO5.
