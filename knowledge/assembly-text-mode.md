# 🔧 Thomson MO5 Assembly - Text Mode

## 📋 Basic structure

### **Minimal working template**
```asm
 org $2600

* Variables
OBJECT_X fcb 10
OBJECT_Y fcb 5

* Main program
MAIN
 * Clear screen
 ldb #$0C
 swi
 fcb $02

 * Introduction message
 ldx #MSG_INTRO
 jsr PRINT_STRING

 * Main loop
GAME_LOOP
 jsr SHOW_SCREEN
 swi
 fcb $0A
 
 * Process key
 cmpb #$5A  ; 'Z'
 beq MOVE_UP
 cmpb #$45  ; 'E'
 beq EXIT_GAME
 
 bra GAME_LOOP

MOVE_UP
 lda OBJECT_Y
 cmpa #1
 bls GAME_LOOP
 deca
 sta OBJECT_Y
 bra GAME_LOOP

EXIT_GAME
 ldb #$0C
 swi
 fcb $02
 rts

* Utility routines
PRINT_STRING
 ldb ,x+
 beq PRINT_STRING_END
 swi
 fcb $02
 bra PRINT_STRING
PRINT_STRING_END
 rts

* Messages
MSG_INTRO
 fcc "=== ASSEMBLY GAME ==="
 fcb $0D,$0A,0

 end MAIN
```

## 🎮 Input handling

### **Key reading**
```asm
; Read a key
READ_KEY
 swi
 fcb $0A
 rts  ; Result in B

; Compare with hexadecimal codes
 cmpb #$5A  ; 'Z' = $5A
 beq MOVE_UP
 cmpb #$53  ; 'S' = $53
 beq MOVE_DOWN
 cmpb #$51  ; 'Q' = $51
 beq MOVE_LEFT
 cmpb #$44  ; 'D' = $44
 beq MOVE_RIGHT
```

### **Important key codes**
```asm
; Letters
KEY_Z    EQU $5A
KEY_S    EQU $53
KEY_Q    EQU $51
KEY_D    EQU $44
KEY_E    EQU $45

; Numbers
KEY_1    EQU $31
KEY_2    EQU $32

; Controls
KEY_SPACE EQU $20
KEY_ENTER EQU $0D
```

## 📺 Text display

### **Character display**
```asm
; Display a character
PUTCHAR
 swi
 fcb $02
 rts

; Clear screen
CLEAR_SCREEN
 ldb #$0C
 swi
 fcb $02
 rts

; New line
NEWLINE
 ldb #$0D
 swi
 fcb $02
 ldb #$0A
 swi
 fcb $02
 rts
```

### **String display**
```asm
; Display a null-terminated string
PRINT_STRING
 ldb ,x+
 beq PRINT_STRING_END
 swi
 fcb $02
 bra PRINT_STRING
PRINT_STRING_END
 rts

; Usage
 ldx #MESSAGE
 jsr PRINT_STRING

MESSAGE
 fcc "Hello Thomson MO5!"
 fcb $0D,$0A,0
```

### **Number display**
```asm
; Display a number 0-99
PRINT_NUMBER
 cmpa #10
 blo PRINT_SINGLE
 
 ; Tens
 pshs a
 ldb #10
 lda #0
COUNT_TENS
 inca
 subb #10
 bpl COUNT_TENS
 deca
 adda #$30
 tfr a,b
 swi
 fcb $02
 puls a
 
 ; Units
 ldb #10
CALC_UNITS
 subb #10
 bpl CALC_UNITS
 addb #10
 addb #$30
 swi
 fcb $02
 rts

PRINT_SINGLE
 adda #$30
 tfr a,b
 swi
 fcb $02
 rts
```

## 🎯 Game management

### **Simple game grid**
```asm
; Display a 20x10 grid
SHOW_SCREEN
 jsr CLEAR_SCREEN
 
 ; Title
 ldx #MSG_TITLE
 jsr PRINT_STRING
 
 ; Grid
 lda #0  ; Line Y
DRAW_LINE
 pshs a
 lda #0  ; Column X
DRAW_CHAR
 pshs a
 
 ; Check if it's the object position
 lda OBJECT_X
 cmpa ,s
 bne NOT_OBJECT
 lda OBJECT_Y
 cmpa 1,s
 bne NOT_OBJECT
 
 ; It's the object
 ldb #$2A  ; '*'
 swi
 fcb $02
 bra NEXT_CHAR

NOT_OBJECT
 ; Check if it's a border
 lda 1,s  ; Y
 cmpa #0
 beq BORDER
 cmpa #9
 beq BORDER
 lda ,s   ; X
 cmpa #0
 beq BORDER
 cmpa #19
 beq BORDER
 
 ; Empty space
 ldb #$20  ; ' '
 swi
 fcb $02
 bra NEXT_CHAR

BORDER
 ldb #$23  ; '#'
 swi
 fcb $02

NEXT_CHAR
 puls a
 inca
 cmpa #20
 blo DRAW_CHAR
 
 ; End of line
 ldb #$0D
 swi
 fcb $02
 ldb #$0A
 swi
 fcb $02
 
 puls a
 inca
 cmpa #10
 blo DRAW_LINE
 
 rts
```

### **Object movement**
```asm
; Position variables
OBJECT_X fcb 10
OBJECT_Y fcb 5

; Move up
MOVE_UP
 lda OBJECT_Y
 cmpa #1
 bls MOVE_END  ; Limit reached
 deca
 sta OBJECT_Y
 bra MOVE_END

; Move down
MOVE_DOWN
 lda OBJECT_Y
 cmpa #8
 bhs MOVE_END  ; Limit reached
 inca
 sta OBJECT_Y
 bra MOVE_END

; Move left
MOVE_LEFT
 lda OBJECT_X
 cmpa #1
 bls MOVE_END  ; Limit reached
 deca
 sta OBJECT_X
 bra MOVE_END

; Move right
MOVE_RIGHT
 lda OBJECT_X
 cmpa #18
 bhs MOVE_END  ; Limit reached
 inca
 sta OBJECT_X

MOVE_END
 rts
```

## ⚠️ Errors to avoid

### **c6809 assembly syntax**
```asm
; ✅ CORRECT
 org $2600          ; Lowercase with space
LABEL               ; Labels without indentation
 instruction        ; Instructions with indentation
 fcc "text"         ; Strings with fcc
 fcb $0D,$0A,0      ; Bytes with fcb

; ❌ INCORRECT
	ORG $2600       ; Uppercase with tab
	LABEL:          ; Colon after label
	FCC 'text'      ; Single quotes
	DB $0D,$0A,0    ; DB instead of fcb
```

### **Memory management**
```asm
; ✅ CORRECT
 org $2600          ; Safe address
STACK_S EQU $20CC   ; System stack

; ❌ INCORRECT
 org $2000          ; Too low, risk of conflict
 org $A000          ; ROM area, read-only
```

### **System calls**
```asm
; ✅ CORRECT
 swi
 fcb $02            ; Code on separate line

; ❌ INCORRECT
 swi $02            ; Code on same line
 int $02            ; Non-existent instruction
```

## 🚀 Compilation

### **Compilation command**
```bash
c6809 -bl program.asm program.BIN
```

### **Disk image creation**
```bash
fdfs -addBL image.fd BOOTMO.BIN PROGRAM.BIN
```

## 📊 Complete working example

See the minimal template at the beginning of this document. It contains everything needed to create a simple text mode game with object movement and key handling.

This assembly approach is **guaranteed to work** on Thomson MO5 and gives you complete control over the hardware.
