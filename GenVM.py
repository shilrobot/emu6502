from Common import getEncodings
import re

codeRegex = re.compile("(\w+)\s+\{\{(.*?)\}\}", re.DOTALL)

vmtab = open("vmtab.txt","rt").read()
codeDict = {}
for match in codeRegex.finditer(vmtab):
	#print match.groups()
	opname = match.group(1).upper()
	code = match.group(2).strip()
	codeDict[opname] = code
	
	
macros = {}
macros['setnz'] = 'N = (M > 127); Z = (M != 0);';	
	

modeHelpers = {'imm':'IMM',
				'zpage':'ZP',
				'zpagex':'ZPX',
				'zpagey':'ZPY',
				'accum':'A',
				'abs':'ABS',
				'absx':'ABSX',
				'absy':'ABSY',
				'ind':'IND',
				'indx':'INDX',
				'indy':'INDY',
				'imp':'IMPL',
				'rel':'REL'}
				
modeSizes = {'imm':2,
				'zpage':2,
				'zpagex':2,
				'zpagey':2,
				'accum':1,
				'abs':3,
				'absx':3,
				'absy':3,
				'ind':3,
				'indx':2,
				'indy':2,
				'imp':1,
				'rel':2}

encodings = getEncodings()
s = ''
for i in encodings:
	if i is None:
		continue
	if not (i.name in codeDict):
		continue
	code = codeDict[i.name]
	
	# TODO: The rest
		
	s += '#define AGEN AGEN_%s\n' % modeHelpers[i.modename]
	s += '#define READ READ_%s\n' % modeHelpers[i.modename]
	s += '#define WRITE WRITE_%s\n' % modeHelpers[i.modename]
	s += '// %s (%s)\n' % (i.name, i.modename)
	s += 'case 0x%02X:\n' % i.opcode
	s += '{\n'
	s += 'Encountered("%s");\n' % i.name
	#s += 'Console.WriteLine("%s %s");\n' % (i.name, i.modename)
	s += 'NPC = (ushort)(PC+%d);\n' % modeSizes[i.modename]
	s += 'AGEN\n'
	s += code+'\n'
	s += '}\n'
	s += 'break;\n\n'
print s

open("vm_out.c","wt").write(s)