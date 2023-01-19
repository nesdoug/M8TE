; example Mode 7 with RLE

.p816
.smart

.include "regs.asm"
.include "variables.asm"
.include "macros.asm"
.include "init.asm"
.include "unrle.asm"




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
	sta VMAIN ; $2115 = set the increment mode +1


	ldx #$0000
	stx VMADDL ; set an address in the vram of $0000
	
; decompress Tiles
	AXY16
	lda #.loword(Tiles)
	ldx #^Tiles
	jsl Unrle
; returns y = size of unpacked data
; and ax = address of UNPACK_ADR    
    jsl DMA_VRAM_M7_TILE
	
	
	
; DMA from Tilemap to VRAM	
	ldx #$0000
	stx VMADDL ; set an address in the vram of $6000
	
; decompress Tilemap
	AXY16
	lda #.loword(Tilemap)
	ldx #^Tilemap
	jsl Unrle
; returns y = size of unpacked data
; and ax = address of UNPACK_ADR    
    jsl DMA_VRAM_M7_MAP    
    
    
    
    
; exits with AXY 16
    
	
    A8
	lda #7 ; mode 7
	sta BGMODE ; $2105
	
; 210b = tilesets for bg 1 and bg 2
; (210c for bg 3 and bg 4)
; steps of $1000 -321-321... bg2 bg1
	stz BG12NBA ; $210b BG 1 and 2 TILES at VRAM address $0000
	
	; 2107 map address bg 1, steps of $400, but -54321yx
	; y/x = map size... 0,0 = 32x32 tiles
	; $7800 / $100 = $78
	lda #$78 ; address $7800
	sta BG1SC ; $2107

	lda #BG1_ON	; only bg 1 is active
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
	
	
	
;jsl here	
DMA_VRAM:
.a16
.i16
; do during forced blank	
; first set VRAM_Addr and VRAM_Inc
; a = source
; x = source bank
; y = length in bytes
	php
	rep #$30 ;axy16
	sta $4302 ; source and 4303
	sep #$20 ;a8
	txa
	sta $4304 ; bank
	lda #$18
	sta $4301 ; destination, vram data
	sty $4305 ; length, and 4306
	lda #1
	sta $4300 ; transfer mode, 2 registers, write once = 2 bytes
	sta $420b ; start dma, channel 0
	plp
	rtl	
    
    
;jsl here	
DMA_VRAM_M7_TILE:
.a16
.i16
; do during forced blank	
; a = source
; x = source bank
; y = length in bytes
	php
	rep #$30 ;axy16
	sta $4302 ; source and 4303
	sep #$20 ;a8
	txa
	sta $4304 ; bank
	lda #$19 
	sta $4301 ; destination, vram data
	sty $4305 ; length, and 4306
    
    lda #V_INC_1 ; the value $80
	sta VMAIN  ; $2115 = set the increment mode +1
    
;	lda #0 ;increment on the 2119 write
	stz $4300 ; transfer mode, 1 register 1 write
    lda #1
	sta $420b ; start dma, channel 0
	plp
	rtl	    
	

DMA_VRAM_M7_MAP:	
; do during forced blank	
; a = source
; x = source bank
; y = length in bytes
	php
	rep #$30 ;axy16
	sta $4302 ; source and 4303
	sep #$20 ;a8
	txa
	sta $4304 ; bank
	lda #$18
	sta $4301 ; destination, vram data
	sty $4305 ; length, and 4306
    
;   lda #0 ;increment on the 2118 write
	stz VMAIN  ; $2115 = set the increment mode +1
    
;	lda #0
	stz $4300 ; transfer mode, 1 register 1 write
    lda #1
	sta $420b ; start dma, channel 0
	plp
	rtl	
    
	
.include "header.asm"	


.segment "RODATA1"

BG_Palette:
; 512 bytes
.incbin "M8TE/dog.pal"

Tiles:
; 8bpp tileset compressed
.incbin "RLE/dogCHR.rle"

Tilemap:
; tilemap compressed
.incbin "RLE/dog128map.rle"



