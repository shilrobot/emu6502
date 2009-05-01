
#define SETNZ Z = (data == 0); N = (data > 127);

#define RD(addr) Read(addr)
#define RDW(addr) ReadWord(addr)

#define LO RD(PC+1)
#define HI RD(PC+2)
#define HILO ReadWord(PC+1)
	 
#define AGEN_IMM	
#define AGEN_ZP		addr = LO;
#define AGEN_ZPX	addr = (ushort)((LO+X)&0xFF);	// Zero page wraps around
#define AGEN_ZPY	addr = (ushort)((LO+Y)&0xFF);	// Zero page wraps around
#define AGEN_A
#define AGEN_ABS	addr = HILO;
#define AGEN_ABSX	addr = (ushort)(HILO+X);
#define AGEN_ABSY	addr = (ushort)(HILO+Y);
// Only used for jump indirect -- simulate JMP INDIRECT page wraparound "bug"
#define AGEN_IND	{  \
						ushort addr1 = HILO; \
						byte addr2_lo = RD(addr1); \
						addr1 = (ushort)((addr1 & 0xFF00) | ((addr1+1)&0xFF)); \
						byte addr2_hi = RD(addr1); \
						addr = (ushort)(addr2_lo | (addr2_hi<<8)); \
					}
#define AGEN_INDX	addr = RDW((ushort)(LO+X));
#define AGEN_INDY	addr = (ushort)(RDW(LO)+Y);
#define AGEN_IMPL
#define AGEN_REL

// TODO: For indirect, the next byte is wraps around to beginning of page! :(

#define READ_IMM 	data = LO;
#define READ_ZP 	data = RD(addr);
#define READ_ZPX 	data = RD(addr);
#define READ_ZPY 	data = RD(addr);
#define READ_A		data = A;
#define READ_ABS 	data = RD(addr);
#define READ_ABSX	data = RD(addr);
#define READ_ABSY	data = RD(addr);
#define READ_IND	data = RD(addr);
#define READ_INDX	data = RD(addr);
#define READ_INDY	data = RD(addr);
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
#define WRITE_IND	
#define WRITE_INDX	Write(addr, data);
#define WRITE_INDY	Write(addr, data);
#define WRITE_IMPL
#define WRITE_REL	// TODO: Hmm

#define BRANCH(cond) if(cond) { byte lo = LO; int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); }

#include "vm_out.c"
