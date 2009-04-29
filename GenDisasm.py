

import re


class InstrEncoding:
	def __init__(self, opcode, name, modename):
		self.opcode = opcode
		self.name = name
		self.modename = modename
		
currInstrName = None

encodings = [None]*256

f = open("opcodetab.txt", "rt")
for line in f.readlines():
	line = line.strip()
	if line == '':
		continue
	if line.startswith('#'):
		continue
	if line.endswith(':'):
		currInstrName = line[:-1]
	else:
		parts = line.split()
		#currInstr.modes[parts[0]] = parts[1:]
		modename = parts[0]
		opcode = int(parts[1],16)
		encodings[opcode] = InstrEncoding(opcode, currInstrName, modename)
f.close()
	
#for i in instrs:
#	print i.name, i.modes

modeHelpers = {'imm':'Immediate()',
				'zpage':'ZeroPage()',
				'zpagex':'ZeroPageIndexedX()',
				'zpagey':'ZeroPageIndexedY()',
				'accum':'Accumulator()',
				'abs':'Absolute()',
				'absx':'AbsoluteIndexedX()',
				'absy':'AbsoluteIndexedY()',
				'ind':'Indirect()',
				'indx':'IndexedIndirectX()',
				'indy':'IndirectIndexedY()',
				'imp':'""',
				'rel':'Relative()'}
							
modeHelpers2 = {'imm':'Immediate',
				'zpage':'ZeroPage',
				'zpagex':'ZeroPageIndexedX',
				'zpagey':'ZeroPageIndexedY',
				'accum':'Accumulator',
				'abs':'Absolute',
				'absx':'AbsoluteIndexedX',
				'absy':'AbsoluteIndexedY',
				'ind':'Indirect',
				'indx':'IndexedIndirectX',
				'indy':'IndirectIndexedY',
				'imp':'Implied',
				'rel':'Relative'}
				
s1 = '\n'
n=0
for i in encodings:
	if i is None:
		s1 += 'null, '
	else:
		s1 += '"%s", '%i.name	
	n+=1
	if n == 8:
		s1 += '\n'
		n=0
	
s2 = '\n'
n=0
for i in encodings:
	if i is None:
		s2 += 'AddressMode.Implied, '
	else:
		s2+= 'AddressMode.%s, '%(modeHelpers2[i.modename])
	n += 1
	if n == 4:
		s1 += '\n'
		n=0
		

	
if 0:				
	s = ''
	for i in instrs:
		for modename,parms in i.modes.items():
			#print i.name,modename,parms
			opcode = parms[0].upper()
			modeHelper = modeHelpers[modename]
			s += '    '*4
			s += 'case 0x%s: name=\"%s\"; operand=%s; break;' % (opcode, i.name.upper(), modeHelper)
			s += '\n'
#print s

def replaceRegion(s, regionName, contents):
	startPlain = "/* BEGIN %s */" % regionName
	endPlain = "/* END %s */" % regionName
	start = re.escape(startPlain)
	end = re.escape(endPlain)
	regex = re.compile(start + "(.*?)" + end, re.IGNORECASE | re.DOTALL)
	return regex.sub(startPlain+contents+endPlain, s)

fsrc = open("Disasm.cs","r")
fileContents = fsrc.read()
fsrc.close()
fileContents = replaceRegion(fileContents, "OPNAMES", s1)
fileContents = replaceRegion(fileContents, "MODES", s2)
#fileContents = replaceRegion(fileContents, "GENERATED", '\n'+s+'            ')
#print fileContents

fdest = open("Disasm.cs","wt")
fdest.write(fileContents)
fdest.close()