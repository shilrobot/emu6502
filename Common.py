import re

class InstrEncoding:
	def __init__(self, opcode, name, modename):
		self.opcode = opcode
		self.name = name
		self.modename = modename
		
def getEncodings():
	currName = None
	encodings = [None]*256
	
	f = open("opcodetab.txt", "rt")
	for line in f.readlines():
		line = line.strip()
		if line == '':
			continue
		if line.startswith('#'):
			continue
		if line.endswith(':'):
			currName = line[:-1]
		else:
			parts = line.split()
			#currInstr.modes[parts[0]] = parts[1:]
			modename = parts[0]
			opcode = int(parts[1],16)
			encodings[opcode] = InstrEncoding(opcode, currName, modename)
	f.close()	
	return encodings

def replaceRegion(s, regionName, contents):
	startPlain = "/* BEGIN %s */" % regionName
	endPlain = "/* END %s */" % regionName
	start = re.escape(startPlain)
	end = re.escape(endPlain)
	regex = re.compile(start + "(.*?)" + end, re.IGNORECASE | re.DOTALL)
	return regex.sub(startPlain+contents+endPlain, s)