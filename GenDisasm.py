from Common import getEncodings, replaceRegion

modeHelpers = {'imm':'Immediate',
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
				
encodings = getEncodings()				
				
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
		s2+= 'AddressMode.%s, '%(modeHelpers[i.modename])
	n += 1
	if n == 4:
		s2 += '\n'
		n=0

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