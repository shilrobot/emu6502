
#define SETNZ Z = (data == 0); N = (data > 127);
#define LO Read(PC+1)
#define HI Read(PC+2)
#define HILO ReadWord(PC+1)
	 
#define AGEN_IMM	
#define AGEN_ZP		addr = LO;
#define AGEN_ZPX	addr = (ushort)((LO+X)&0xFF);
#define AGEN_ZPY	addr = (ushort)((LO+Y)&0xFF);
#define AGEN_A
#define AGEN_ABS	addr = HILO;
#define AGEN_ABSX	addr = (ushort)(HILO+X);
#define AGEN_ABSY	addr = (ushort)(HILO+Y);
#define AGEN_IND	addr = ReadWord(HILO);
#define AGEN_INDX	addr = ReadWord((LO+X) & 0xFFFF);
#define AGEN_INDY	addr = (ushort)(ReadWord(LO)+Y);
#define AGEN_IMPL
#define AGEN_REL

#define READ_IMM 	data = LO;
#define READ_ZP 	data = Read(addr);
#define READ_ZPX 	data = Read(addr);
#define READ_ZPY 	data = Read(addr);
#define READ_A		data = A;
#define READ_ABS 	data = Read(addr);
#define READ_ABSX	data = Read(addr);
#define READ_ABSY	data = Read(addr);
#define READ_IND	data = Read(addr);
#define READ_INDX	data = Read(addr);
#define READ_INDY	data = Read(addr);
#define READ_IMPL
#define READ_REL	// TODO: Hmm

#define WRITE_IMM 	
#define WRITE_ZP 	Write(addr, data);
#define WRITE_ZPX 	Write(addr, data);
#define WRITE_ZPY 	Write(addr, data);
#define WRITE_A		A = data;
#define WRITE_ABS 	Write(addr, data);
#define WRITE_ABSX	Write(addr, data);
#define WRITE_ABSY	Write(addr, data);
#define WRITE_IND	Write(addr, data);
#define WRITE_INDX	Write(addr, data);
#define WRITE_INDY	Write(addr, data);
#define WRITE_IMPL
#define WRITE_REL	// TODO: Hmm

#define BRANCH(cond) if(cond) { byte lo = LO; int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); }

#include "vm_out.c"
