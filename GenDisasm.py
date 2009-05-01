from Common import getEncodings, replaceRegion

modeSizes = {'imm':'2',
				'zpage':'2',
				'zpagex':'2',
				'zpagey':'2',
				'accum':'1',
				'abs':'3',
				'absx':'3',
				'absy':'3',
				'ind':'3',
				'indx':'2',
				'indy':'2',
				'imp':'1',
				'rel':'2'}
				
modeTypes = {'imm':0,
				'zpage':2,
				'zpagex':5,
				'zpagey':8,
				'accum':0,
				'abs':3,
				'absx':7,
				'absy':6,
				'ind':0,
				'indx':1,
				'indy':4,
				'imp':0,
				'rel':0}
				
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

s3 = '\n'
n=0
for i in encodings:
	if i is None:
		s3 += '0,'
	else:
		s3 += '%d,'%(modeTypes[i.modename])
	n+=1
	if n == 16:
		s3 += '\n'
		n=0
print s3
		
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