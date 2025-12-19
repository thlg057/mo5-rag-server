# 🔧 Thomson MO5 C - Text Mode

## 📋 Basic structure

### **Minimal working template**
```c
/*
 * Thomson MO5 C Program - Text Mode
 * Compiled with CMOC
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

// Global variables
unsigned char object_x = 10;
unsigned char object_y = 5;

// String display
void print_string(const char *str) {
    while (*str) {
        mo5_putchar(*str);
        str++;
    }
}

// Number display 0-99
void print_number(unsigned char num) {
    if (num >= 10) {
        mo5_putchar('0' + (num / 10));
    }
    mo5_putchar('0' + (num % 10));
}

// Game screen display
void show_screen(void) {
    unsigned char x, y;
    
    mo5_clear_screen();
    print_string("=== C THOMSON MO5 GAME ===\r\n\r\n");
    
    // 20x10 grid
    for (y = 0; y < 10; y++) {
        for (x = 0; x < 20; x++) {
            if (x == object_x && y == object_y) {
                mo5_putchar('*');  // Object
            } else if (x == 0 || x == 19 || y == 0 || y == 9) {
                mo5_putchar('#');  // Border
            } else {
                mo5_putchar(' ');  // Empty
            }
        }
        mo5_putchar('\r');
        mo5_putchar('\n');
    }
    
    print_string("\r\nPosition: X=");
    print_number(object_x);
    print_string(" Y=");
    print_number(object_y);
    print_string("\r\n");
    print_string("Z=Up S=Down Q=Left D=Right E=Quit\r\n");
}

// Object movement
void move_object(char direction) {
    switch (direction) {
        case 'Z':
        case 'z':
            if (object_y > 1) object_y--;
            break;
        case 'S':
        case 's':
            if (object_y < 8) object_y++;
            break;
        case 'Q':
        case 'q':
            if (object_x > 1) object_x--;
            break;
        case 'D':
        case 'd':
            if (object_x < 18) object_x++;
            break;
    }
}

// Main program
int main(void) {
    char key;
    
    // Game loop
    while (1) {
        show_screen();
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
        }
    }
    
exit_game:
    mo5_clear_screen();
    print_string("Goodbye!\r\n");
    return 0;
}
```

## 🎮 Input handling

### **Key reading**
```c
// Read a key (blocking)
char mo5_getchar(void) {
    asm {
        swi
        fcb $0A
        tfr b,a
    }
}

// Usage
char key = mo5_getchar();
if (key == 'Z' || key == 'z') {
    // Processing
}
```

### **Special key codes handling**
```c
// Special key codes
#define KEY_SPACE   32
#define KEY_ENTER   13
#define KEY_ESC     27

// Processing function
void handle_key(char key) {
    switch (key) {
        case 'Z':
        case 'z':
            move_up();
            break;
        case KEY_SPACE:
            fire();
            break;
        case KEY_ENTER:
            select();
            break;
    }
}
```

## 📺 Text display

### **Basic display**
```c
// Display a character
void mo5_putchar(char c) {
    asm {
        ldb c
        swi
        fcb $02
    }
}

// Clear screen
void mo5_clear_screen(void) {
    mo5_putchar(12);  // Form Feed
}

// New line
void newline(void) {
    mo5_putchar('\r');
    mo5_putchar('\n');
}
```

### **String display**
```c
// Display a string
void print_string(const char *str) {
    while (*str) {
        mo5_putchar(*str);
        str++;
    }
}

// Display with new line
void print_line(const char *str) {
    print_string(str);
    newline();
}

// Usage
print_string("Hello ");
print_line("Thomson MO5!");
```

### **Number display**
```c
// Display a number 0-255
void print_number(unsigned char num) {
    if (num >= 100) {
        mo5_putchar('0' + (num / 100));
        num %= 100;
    }
    if (num >= 10) {
        mo5_putchar('0' + (num / 10));
    }
    mo5_putchar('0' + (num % 10));
}

// Display a 16-bit number
void print_word(unsigned int num) {
    unsigned char digits[5];
    unsigned char i = 0;
    
    if (num == 0) {
        mo5_putchar('0');
        return;
    }
    
    while (num > 0) {
        digits[i++] = num % 10;
        num /= 10;
    }
    
    while (i > 0) {
        mo5_putchar('0' + digits[--i]);
    }
}
```

## 🎯 Game structures

### **Game variables**
```c
// Player structure
typedef struct {
    unsigned char x;
    unsigned char y;
    unsigned char lives;
    unsigned int score;
} Player;

// Global variables
Player player = {10, 5, 3, 0};
unsigned char game_running = 1;

// Initialization
void init_game(void) {
    player.x = 10;
    player.y = 5;
    player.lives = 3;
    player.score = 0;
    game_running = 1;
}
```

### **Game loop**
```c
// Main game loop
void game_loop(void) {
    char key;
    
    init_game();
    
    while (game_running) {
        // Display
        show_screen();
        
        // Input
        key = mo5_getchar();
        
        // Processing
        handle_input(key);
        
        // Game logic
        update_game();
        
        // Check game over
        if (player.lives == 0) {
            game_running = 0;
        }
    }
    
    show_game_over();
}
```

### **User interface**
```c
// Interface display
void show_interface(void) {
    print_string("Score: ");
    print_word(player.score);
    print_string("  Lives: ");
    print_number(player.lives);
    newline();
}

// Main menu
void show_menu(void) {
    mo5_clear_screen();
    print_line("=== MAIN MENU ===");
    print_line("");
    print_line("1. New game");
    print_line("2. Instructions");
    print_line("3. Quit");
    print_line("");
    print_string("Your choice? ");
}

// Menu handling
char handle_menu(void) {
    char choice;
    
    show_menu();
    choice = mo5_getchar();
    
    switch (choice) {
        case '1':
            return 1;  // New game
        case '2':
            show_instructions();
            return 0;  // Stay in menu
        case '3':
            return -1; // Quit
        default:
            return 0;  // Invalid choice
    }
}
```

## ⚠️ Errors to avoid

### **Forbidden functions**
```c
// ❌ FORBIDDEN - These functions don't exist on Thomson MO5
printf("Hello");           // No printf
scanf("%d", &num);         // No scanf
puts("Hello");             // No puts
getchar();                 // No standard getchar
malloc(100);               // No malloc
free(ptr);                 // No free
#include <stdio.h>         // No standard libraries
#include <stdlib.h>        // No standard libraries
```

### **Correct usage**
```c
// ✅ CORRECT - Use Thomson MO5 functions
mo5_putchar('H');          // Character display
print_string("Hello");     // String display
key = mo5_getchar();       // Character reading
```

### **Memory management**
```c
// ❌ FORBIDDEN
char *buffer = malloc(1000);  // No malloc

// ✅ CORRECT
char buffer[1000];            // Static arrays
static char big_buffer[2000]; // Static variables
```

### **Data types**
```c
// ✅ RECOMMENDED - Explicit types
unsigned char x;     // 0-255
signed char y;       // -128 to 127
unsigned int score;  // 0-65535

// ⚠️ AVOID - Ambiguous types
int value;           // Compiler-dependent size
long big_num;        // May not be supported
```

## 🚀 Compilation

### **CMOC command**
```bash
cmoc --thommo --org=2600 -o program.BIN program.c
```

### **Important options**
- `--thommo` : Target Thomson MO5/MO6
- `--org=2600` : Load address
- `-o` : Output file

### **Disk image creation**
```bash
fdfs -addBL image.fd BOOTMO.BIN PROGRAM.BIN
```

## 📊 Advantages of C

### **Compared to assembly**
- **Readability** : Code easier to understand
- **Maintenance** : Simpler modifications
- **Structures** : Complex data types
- **Functions** : Code reuse

### **Limitations**
- **Size** : Larger code than assembly
- **Performance** : Slightly slower
- **Control** : Less hardware control

## 🎮 Complete working example

The minimal template at the beginning of this document contains a complete game with:
- 20x10 game grid
- Movable object with ZQSD
- Position display
- Proper input/output handling

This code is **tested and functional** on Thomson MO5.

C with CMOC is an excellent compromise between development ease and performance on Thomson MO5.
