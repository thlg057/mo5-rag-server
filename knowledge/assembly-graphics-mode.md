# 🎨 Thomson MO5 Assembly - Graphics Mode

## 📋 Basic concepts

### **Thomson MO5 video system**
Thomson MO5 uses a **dual-plane** video system:
- **Color plane** : Defines pixel colors
- **Form plane** : Defines shape (pixels on/off)

### **Video control register**
```asm
; Control register address
VIDEO_REG EQU $A7C0

; Select color plane
VIDEOC
 lda VIDEO_REG
 anda #%11111110  ; Bit 0 = 0
 sta VIDEO_REG
 rts

; Select form plane
VIDEOF
 lda VIDEO_REG
 ora #%00000001   ; Bit 0 = 1
 sta VIDEO_REG
 rts
```

## 🖼️ Video memory organization

### **Screen address calculation**
```asm
; Mode 320x200 : 40 bytes per line, 2 pixels per byte
; Address = Y * 40 + (X / 2)

CALC_SCREEN_ADDR
 ; Input: A = X, B = Y
 ; Output: X = screen address
 pshs a,b
 
 ; Y * 40
 lda #40
 mul          ; D = Y * 40
 tfr d,x      ; X = Y * 40
 
 ; Add X / 2
 puls a,b
 lsra         ; A = X / 2
 leax a,x     ; X = (Y * 40) + (X / 2)
 
 rts
```

### **Screen dimensions**
```asm
SCREEN_WIDTH  EQU 40    ; 40 bytes per line
SCREEN_HEIGHT EQU 200   ; 200 lines
PIXELS_PER_BYTE EQU 2   ; 2 pixels per byte
```

## 🎨 Sprite display

### **Simple 16x16 sprite**
```asm
; Display a 16x16 square
; Input: X = screen address
DRAW_SPRITE_16x16
 pshs x
 
 ; Color plane
 bsr VIDEOC
 lda #$77     ; White on white background
 ldb #40      ; Line skip
 bsr DRAW_SQUARE_COLOR
 
 ; Form plane
 bsr VIDEOF
 puls x       ; Restore address
 lda #$FF     ; Full pixels
 ldb #40      ; Line skip
 bsr DRAW_SQUARE_FORM
 
 rts

DRAW_SQUARE_COLOR
 ; 16 lines of 2 bytes
 ldy #16
DRAW_COLOR_LINE
 sta ,x
 sta 1,x
 abx          ; X = X + B (next line)
 leay -1,y
 bne DRAW_COLOR_LINE
 rts

DRAW_SQUARE_FORM
 ; 16 lines of 2 bytes
 ldy #16
DRAW_FORM_LINE
 sta ,x
 sta 1,x
 abx          ; X = X + B (next line)
 leay -1,y
 bne DRAW_FORM_LINE
 rts
```

### **Background save/restore**
```asm
; Buffer to save background (32 bytes)
BUFFER_FOND BSZ 32

; Save background of a 16x16 sprite
SAVE_BACKGROUND
 ; Input: X = screen address
 pshs x
 ldy #BUFFER_FOND
 
 ; Save color
 bsr VIDEOC
 ldb #40
 bsr SAVE_16x16_DATA
 
 ; Save form
 bsr VIDEOF
 puls x       ; Restore address
 ldb #40
 bsr SAVE_16x16_DATA
 
 rts

SAVE_16x16_DATA
 ; 16 lines of 2 bytes
 lda #16
SAVE_LINE
 ldu ,x       ; Read 2 bytes
 stu ,y++     ; Save
 abx          ; Next line
 deca
 bne SAVE_LINE
 rts

; Background restoration
RESTORE_BACKGROUND
 ; Input: X = screen address
 pshs x
 ldy #BUFFER_FOND
 
 ; Restore color
 bsr VIDEOC
 ldb #40
 bsr RESTORE_16x16_DATA
 
 ; Restore form
 bsr VIDEOF
 puls x       ; Restore address
 ldb #40
 bsr RESTORE_16x16_DATA
 
 rts

RESTORE_16x16_DATA
 ; 16 lines of 2 bytes
 lda #16
RESTORE_LINE
 ldu ,y++     ; Read from buffer
 stu ,x       ; Restore to screen
 abx          ; Next line
 deca
 bne RESTORE_LINE
 rts
```

## 🎮 Smooth movement

### **Complete movement structure**
```asm
; Position variables
OBJECT_X fcb 160  ; X position (screen center)
OBJECT_Y fcb 100  ; Y position (screen center)
OLD_X    fcb 160  ; Old X position
OLD_Y    fcb 100  ; Old Y position

; Object display
SHOW_OBJECT
 ; Calculate screen address
 lda OBJECT_X
 ldb OBJECT_Y
 bsr CALC_SCREEN_ADDR
 
 ; Save background and display sprite
 bsr SAVE_BACKGROUND
 bsr DRAW_SPRITE_16x16
 rts

; Object erasing
HIDE_OBJECT
 ; Calculate old address
 lda OLD_X
 ldb OLD_Y
 bsr CALC_SCREEN_ADDR
 
 ; Restore background
 bsr RESTORE_BACKGROUND
 rts

; Object movement
MOVE_OBJECT
 ; Save old position
 lda OBJECT_X
 sta OLD_X
 lda OBJECT_Y
 sta OLD_Y
 
 ; Erase old position
 bsr HIDE_OBJECT
 
 ; Display new position
 bsr SHOW_OBJECT
 rts
```

### **Boundary checking**
```asm
; Movement with boundary checking
MOVE_UP
 lda OBJECT_Y
 cmpa #8      ; Top limit (16x16 sprite)
 bls MOVE_END
 suba #5      ; Move 5 pixels
 sta OBJECT_Y
 bsr MOVE_OBJECT
MOVE_END
 rts

MOVE_DOWN
 lda OBJECT_Y
 cmpa #184    ; Bottom limit (200-16)
 bhs MOVE_END
 adda #5
 sta OBJECT_Y
 bsr MOVE_OBJECT
 rts

MOVE_LEFT
 lda OBJECT_X
 cmpa #8      ; Left limit
 bls MOVE_END
 suba #5
 sta OBJECT_X
 bsr MOVE_OBJECT
 rts

MOVE_RIGHT
 lda OBJECT_X
 cmpa #304    ; Right limit (320-16)
 bhs MOVE_END
 adda #5
 sta OBJECT_X
 bsr MOVE_OBJECT
 rts
```

## 🎨 Colored sprites

### **Sprite with custom colors**
```asm
; Color data for sprite
SPRITE_COLORS
 fcb $11,$11  ; Red on red background
 fcb $22,$22  ; Green on green background
 fcb $33,$33  ; Yellow on yellow background
 fcb $44,$44  ; Blue on blue background
 ; ... 12 more lines

; Form data for sprite
SPRITE_FORMS
 fcb $FF,$FF  ; Full line
 fcb $C3,$C3  ; Line with holes
 fcb $81,$81  ; Border
 fcb $81,$81  ; Border
 ; ... 12 more lines

; Colored sprite display
DRAW_COLORED_SPRITE
 ; Input: X = screen address
 pshs x
 
 ; Color plane
 bsr VIDEOC
 ldy #SPRITE_COLORS
 ldb #40
 bsr DRAW_SPRITE_DATA
 
 ; Form plane
 bsr VIDEOF
 puls x
 ldy #SPRITE_FORMS
 ldb #40
 bsr DRAW_SPRITE_DATA
 
 rts

DRAW_SPRITE_DATA
 ; 16 lines of data
 lda #16
DRAW_DATA_LINE
 ldu ,y++     ; Read 2 bytes of data
 stu ,x       ; Write to screen
 abx          ; Next line
 deca
 bne DRAW_DATA_LINE
 rts
```

## 🎯 Complete program

### **Main structure**
```asm
 org $2600

* Variables
OBJECT_X fcb 160
OBJECT_Y fcb 100
OLD_X    fcb 160
OLD_Y    fcb 100

* Main program
MAIN
 * Initialization
 bsr INIT_GRAPHICS
 bsr SHOW_OBJECT
 
GAME_LOOP
 * Read key
 swi
 fcb $0A
 
 * Process movement
 cmpb #$5A  ; 'Z'
 beq MOVE_UP
 cmpb #$53  ; 'S'
 beq MOVE_DOWN
 cmpb #$51  ; 'Q'
 beq MOVE_LEFT
 cmpb #$44  ; 'D'
 beq MOVE_RIGHT
 cmpb #$45  ; 'E'
 beq EXIT_GAME
 
 bra GAME_LOOP

EXIT_GAME
 * Cleanup and exit
 bsr HIDE_OBJECT
 rts

INIT_GRAPHICS
 * Graphics mode initialization
 * (No screen clear to avoid flickering)
 rts

* Include all routines defined above
* (CALC_SCREEN_ADDR, VIDEOC, VIDEOF, etc.)

 end MAIN
```

## ⚠️ Errors to avoid

### **Video plane management**
```asm
; ❌ INCORRECT - Forgetting to change plane
 bsr DRAW_SPRITE_COLOR
 bsr DRAW_SPRITE_FORM    ; Still in color mode!

; ✅ CORRECT - Change plane between color and form
 bsr VIDEOC
 bsr DRAW_SPRITE_COLOR
 bsr VIDEOF
 bsr DRAW_SPRITE_FORM
```

### **Background saving**
```asm
; ❌ INCORRECT - Not saving background
 bsr DRAW_SPRITE         ; Overwrites background permanently

; ✅ CORRECT - Always save before displaying
 bsr SAVE_BACKGROUND
 bsr DRAW_SPRITE
 ; Later...
 bsr RESTORE_BACKGROUND
```

### **Address calculation**
```asm
; ❌ INCORRECT - Wrong address calculation
 lda OBJECT_X
 ldb OBJECT_Y
 mul              ; Y * X instead of Y * 40

; ✅ CORRECT - Correct formula
 lda #40
 ldb OBJECT_Y
 mul              ; Y * 40
 adda OBJECT_X
 lsra             ; + (X / 2)
```

## 🚀 Compilation

### **Assembly**
```bash
c6809 -bl graphics.asm graphics.BIN
```

### **Image creation**
```bash
fdfs -addBL graphics.fd BOOTMO.BIN GRAPHICS.BIN
```

## 📊 Performance

### **Advantages**
- **Complete control** over display
- **Maximum performance**
- **No flickering** with proper management

### **Complexity**
- **Manual management** of video planes
- **Address calculations** to implement
- **Background saving** mandatory

This assembly approach gives you **complete control** over Thomson MO5 graphics capabilities.
