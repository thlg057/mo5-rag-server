# ⚠️ Thomson MO5 Common Errors

## 🚫 C programming errors

### **Using non-existent functions**
```c
// ❌ FORBIDDEN - These functions don't exist on Thomson MO5
printf("Hello World\n");           // No printf
scanf("%d", &number);              // No scanf
puts("Message");                   // No puts
getchar();                         // No standard getchar
malloc(1000);                      // No malloc
free(ptr);                         // No free
strlen(str);                       // No strlen
strcpy(dest, src);                 // No strcpy
#include <stdio.h>                 // No standard libraries
#include <stdlib.h>                // No standard libraries
#include <string.h>                // No standard libraries

// ✅ CORRECT - Use Thomson MO5 functions
mo5_putchar('H');                  // Character display
print_string("Hello World\r\n");   // String display
key = mo5_getchar();               // Character reading
char buffer[1000];                 // Static arrays
```

### **Incorrect memory management**
```c
// ❌ FORBIDDEN - Dynamic allocation
char *buffer = malloc(size);       // malloc doesn't exist
free(buffer);                      // free doesn't exist

// ✅ CORRECT - Static allocation
char buffer[1000];                 // Static array
static char big_buffer[5000];      // Static variable
```

### **Problematic data types**
```c
// ⚠️ AVOID - Ambiguous types
int value;                         // Compiler-dependent size
long big_number;                   // May not be supported
float decimal;                     // Slow floating-point arithmetic
double precision;                  // Very slow on 6809

// ✅ RECOMMENDED - Explicit types
unsigned char byte_value;          // 0-255
signed char signed_byte;           // -128 to 127
unsigned int word_value;           // 0-65535
signed int signed_word;            // -32768 to 32767
```

## 🔧 Assembly errors

### **Incorrect c6809 syntax**
```asm
; ❌ INCORRECT - Invalid syntax
	ORG $2600                   ; Uppercase with tab
LABEL:                          ; Colon after label
	MOV A,B                     ; Non-existent instruction
	FCC 'text'                  ; Single quotes
	DB $0D,$0A,0                ; DB instead of fcb
	SWI $02                     ; Code on same line

; ✅ CORRECT - c6809 syntax
 org $2600                      ; Lowercase with space
LABEL                           ; Label without colon
 tfr a,b                        ; Correct instruction
 fcc "text"                     ; Double quotes
 fcb $0D,$0A,0                  ; fcb for bytes
 swi                            ; SWI on separate line
 fcb $02                        ; Code on next line
```

### **Address management**
```asm
; ❌ INCORRECT - Dangerous addresses
 org $2000                      ; Too low, risk of conflict
 org $A000                      ; ROM area, read-only
 org $0000                      ; Video memory

; ✅ CORRECT - Safe addresses
 org $2600                      ; Safe area for programs
 org $2100                      ; Temporary buffer
```

### **Incorrect system calls**
```asm
; ❌ INCORRECT - Invalid syntax
 int $02                        ; Non-existent instruction
 call PUTCHAR                   ; No direct call
 swi $02                        ; Code on same line

; ✅ CORRECT - Thomson MO5 system calls
 swi                            ; Software interrupt
 fcb $02                        ; System code on separate line
 jsr ROUTINE                    ; Subroutine call
```

## 🖼️ Graphics errors

### **Video plane management**
```c
// ❌ INCORRECT - Forgetting to change plane
videoc_a();
draw_sprite_color();
draw_sprite_form();             // Still in color mode!

// ✅ CORRECT - Change plane between color and form
videoc_a();
draw_sprite_color();
videof();                       // Change to form mode
draw_sprite_form();
```

### **Background saving**
```c
// ❌ INCORRECT - Not saving background
draw_sprite(screen_addr);       // Overwrites background permanently

// ✅ CORRECT - Always save before displaying
save_background(screen_addr);
draw_sprite(screen_addr);
// Later...
restore_background(screen_addr);
```

### **Screen address calculation**
```c
// ❌ INCORRECT - Wrong formulas
unsigned int addr = x * y;                    // Incorrect formula
unsigned int addr = y * 320 + x;              // Wrong width
unsigned int addr = y * 40 + x;               // Missing division by 2

// ✅ CORRECT - Thomson MO5 formula
unsigned int addr = (unsigned int)y * 40 + (x >> 1);  // Correct
```

### **Screen boundaries**
```c
// ❌ INCORRECT - No boundary checking
object_x = 350;                 // Exceeds screen width (320)
object_y = 250;                 // Exceeds screen height (200)

// ✅ CORRECT - Check boundaries
if (object_x <= 304) object_x += 5;  // 320 - 16 = 304 (16x16 sprite)
if (object_y <= 184) object_y += 5;  // 200 - 16 = 184 (16x16 sprite)
```

## 💾 Compilation errors

### **Common CMOC errors**
```bash
# Error: "undefined reference to printf"
# ✅ Solution: Don't use printf, use mo5_putchar

# Error: "undefined reference to malloc"
# ✅ Solution: Use static arrays

# Error: "org directive not supported"
# ✅ Solution: Use --org=2600 as compilation option

# Error: "target not supported"
# ✅ Solution: Use --thommo to target Thomson MO5
```

### **Common c6809 errors**
```bash
# Error: "undefined symbol"
# ✅ Solution: Check label spelling

# Error: "branch out of range"
# ✅ Solution: Use JMP instead of BRA for long distances

# Error: "illegal addressing mode"
# ✅ Solution: Check addressing mode syntax
```

### **Common fdfs errors**
```bash
# Error: "BOOTMO.BIN not found"
# ✅ Solution: Check path to BOOTMO.BIN

# Error: "invalid BIN file"
# ✅ Solution: Check that .BIN file has correct header

# Error: "disk image full"
# ✅ Solution: Create new image or use -force
```

## 🎮 Game logic errors

### **Infinite loop**
```c
// ❌ INCORRECT - Loop without exit
while (1) {
    // No exit condition
}

// ✅ CORRECT - Loop with exit
while (game_running) {
    key = mo5_getchar();
    if (key == 'E') game_running = 0;
}
```

### **Screen flickering**
```c
// ❌ INCORRECT - Complete clear each frame
void update_screen() {
    mo5_clear_screen();         // Causes flickering
    draw_everything();
}

// ✅ CORRECT - Selective update
void update_screen() {
    hide_object();              // Erase only object
    show_object();              // Redraw at new position
}
```

### **Key handling**
```c
// ❌ INCORRECT - No uppercase/lowercase handling
if (key == 'z') move_up();     // Only lowercase

// ✅ CORRECT - Handle both cases
if (key == 'Z' || key == 'z') move_up();
```

## 🔍 Debugging

### **Debugging techniques**
```c
// Variable display for debugging
void debug_print_position() {
    print_string("X=");
    print_number(object_x);
    print_string(" Y=");
    print_number(object_y);
    mo5_putchar('\r');
    mo5_putchar('\n');
}

// Visual breakpoints
void debug_checkpoint(unsigned char checkpoint) {
    print_string("Checkpoint ");
    print_number(checkpoint);
    mo5_putchar('\r');
    mo5_putchar('\n');
    mo5_getchar();  // Wait for key
}
```

### **Memory checking**
```c
// Check array overflows
#define BUFFER_SIZE 100
unsigned char buffer[BUFFER_SIZE];
unsigned char index = 0;

void safe_write(unsigned char value) {
    if (index < BUFFER_SIZE) {
        buffer[index++] = value;
    } else {
        print_string("ERROR: Buffer full!\r\n");
    }
}
```

## 📊 Best practices

### **Code organization**
```c
// ✅ CORRECT - Well-organized code
// 1. Definitions and constants
#define MAX_OBJECTS 10
#define SCREEN_WIDTH 320

// 2. Global variables
unsigned char object_x, object_y;

// 3. Utility functions
void mo5_putchar(char c) { /* ... */ }

// 4. Game functions
void move_object() { /* ... */ }

// 5. Main program
int main(void) { /* ... */ }
```

### **Useful comments**
```c
// ✅ CORRECT - Explanatory comments
// Thomson MO5 screen address calculation (320x200, 40 bytes/line)
unsigned int addr = (unsigned int)y * 40 + (x >> 1);

// Save background before sprite display (prevents overwriting)
save_background(addr);
```

### **Progressive testing**
```c
// ✅ CORRECT - Test step by step
// 1. First test simple display
print_string("Display test\r\n");

// 2. Then test key reading
char key = mo5_getchar();

// 3. Next test movement
move_object(key);

// 4. Finally test graphics
draw_sprite(addr);
```

By avoiding these common errors, you'll develop more efficiently on Thomson MO5!
