


class Instr:
	def __init__(self, name):
		self.name = name
		self.modes = {} # Should be a mapping of modename -> [bytecode], later might have cycle count or other info
		
instrs = []		
currInstr = None

f = open("opcodetab.txt", "rt")
for line in f.readlines():
	line = line.strip()
	if line == '':
		continue
	if line.startswith('#'):
		continue
	if line.endswith(':'):
		if currInstr is not None:
			instrs.append(currInstr)
		currInstr = Instr(line[:-1])
	else:
		parts = line.split()
		currInstr.modes[parts[0]] = parts[1:]
f.close()

if currInstr is not None:
	instrs.append(currInstr)
	
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

fsrc = open("Disasm.cs","r")
fileContents = fsrc.read()
fsrc.close()
import re
regex = re.compile(r"/\* BEGIN GENERATED \*/(.*?)/\* END GENERATED \*/", re.M | re.DOTALL)
fileContents = regex.sub('/* BEGIN GENERATED */\n'+s+'            /* END GENERATED */', fileContents)
print fileContents

fdest = open("Disasm.cs","wt")
fdest.write(fileContents)
fdest.close()