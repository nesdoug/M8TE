; example Mode 7

.p816
.smart

.include "regs.asm"
.include "variables.asm"
.include "macros.asm"
.include "init.asm"




.segment "ZEROPAGE"

address_v:  .res 2
map_ptr:    .res 4
loop_cnt:   .res 2


.segment "CODE"

; enters here in forced blank
Main:
.a16 ; the setting from init code
.i16
	phk
	plb
	

	
; DMA from BG_Palette to CGRAM
	A8
	stz CGADD ; $2121 cgram address = zero
	
	stz $4300 ; transfer mode 0 = 1 register write once
	lda #$22  ; $2122
	sta $4301 ; destination, cgram data
	ldx #.loword(BG_Palette)
	stx $4302 ; source
	lda #^BG_Palette
	sta $4304 ; bank
	ldx #512
	stx $4305 ; length
	lda #1
	sta MDMAEN ; $420b start dma, channel 0
	
    
	
; DMA from Tiles to VRAM	
	lda #V_INC_1 ; the value $80
	sta VMAIN  ; $2115 = set the increment mode +1
    
	ldx #$0000
	stx VMADDL ; $2116 set an address in the vram of $0000
	
;	lda #0
	stz $4300 ; transfer mode, 1 register 1 write
			  ; $2119 only written to
	lda #$19  ; $2119
	sta $4301 ; destination, vram data
	ldx #.loword(Tiles)
	stx $4302 ; source
	lda #^Tiles
	sta $4304 ; bank
	ldx #$4000
	stx $4305 ; length
	lda #1
	sta MDMAEN ; $420b start dma, channel 0
    
 
	
	
	
; DMA from Tilemap to VRAM	
    stz VMAIN  ; $2115 = set the increment mode +1
				;NOTE - increment address after 2118
				;will fill only the low bytes of VRAM
    
	ldx #$0000
	stx VMADDL ; $2116 set an address in the vram of $0000
	
;   jsr LoadMapDMA
    jsr LoadMap2
	
	
; a is still 8 bit.
	lda #7 ; mode 7
	sta BGMODE ; $2105
	
; 210b = tilesets for bg 1 and bg 2
; (210c for bg 3 and bg 4)
; steps of $1000 -321-321... bg2 bg1
	stz BG12NBA ; $210b BG 1 and 2 TILES at VRAM address $0000
	
	; 2107 map address bg 1, steps of $400... -54321yx
	; y/x = map size... 0,0 = 32x32 tiles
	; $7800 / $100 = $78
;	lda #$78 ; bg1 map at VRAM address $7800
;	sta BG1SC ; $2107

	lda #BG1_ON	; $01 = only bg 1 is active
	sta TM ; $212c
    
;    lda #$ff ;shift the screen down 1 pixel
 ;   sta BG1VOFS
  ;  sta BG1VOFS
  
  
  
;zoom in 200%  
;2 writes, low then high
    lda #$80
	sta M7A
	lda #0
	sta M7A
    
;note M7B and M7C were set to zero in init code    
    
    lda #$80
	sta M7D
	lda #0
	sta M7D
    
    lda #V_INC_1 ; the value $80
	sta VMAIN ; back to "normal"
	
	lda #FULL_BRIGHT ; $0f = turn the screen on (end forced blank)
	sta INIDISP ; $2100


Infinite_Loop:	
	A8
	XY16
	jsr Wait_NMI
	
	;code goes here

	jmp Infinite_Loop
	
    
LoadMapDMA: 
;for the 128x128 map
    php
    A8
    XY16
   
 ;	lda #0
	stz $4300 ; transfer mode, 2 registers 1 write
			  ; $2118 and $2119 are a pair Low/High
	lda #$18  ; $2118
	sta $4301 ; destination, vram data
	ldx #.loword(Tilemap)
	stx $4302 ; source
	lda #^Tilemap
	sta $4304 ; bank
	ldx #$4000
	stx $4305 ; length
	lda #1
	sta MDMAEN ; $420b start dma, channel 0	
    plp
    rts
    
    
LoadMap2: 
;for the 32x32 map
;address_v:  .res 2
;map_ptr:    .res 4
    php
    A8
    XY16
    ldx #$0000
    stx address_v
    stx VMADDL
    
    ldx #.loword(Tilemap2)
    stx map_ptr
    ldx #^Tilemap2
    stx map_ptr+2
    
    stz $4300
; transfer mode, 2 registers 1 write
; $2118 and $2119 are a pair Low/High  

    ldx #32
    stx loop_cnt
    ldy #0
@loop:
    lda [map_ptr], y
    sta $2118
    iny
    dex
    bne @loop
    A16
    lda address_v ; jump down a row
    clc
    adc #128
    sta address_v
    sta VMADDL
    A8
    ldx #32
    dec loop_cnt
    bne @loop
    plp
    rts    
	
	
Wait_NMI:
.a8
.i16
;should work fine regardless of size of A
	lda in_nmi ;load A register with previous in_nmi
@check_again:	
	WAI ;wait for an interrupt
	cmp in_nmi	;compare A to current in_nmi
				;wait for it to change
				;make sure it was an nmi interrupt
	beq @check_again
	rts
	
	

.include "header.asm"	


.segment "RODATA1"

BG_Palette:
; 512 bytes
.incbin "M8TE/dog.pal"


Tilemap:
; $4000 bytes
.incbin "M8TE/dog128.map"


.segment "RODATA2"

Tiles:
; 8bpp tileset
.incbin "M8TE/dog7.chr"

.segment "RODATA3"


Tilemap2:
; $4000 bytes
.incbin "M8TE/dog32.map"



