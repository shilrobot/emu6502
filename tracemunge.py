import sys

remove = ["FD36 10", "FD33 AD", "FD2E AD", "FD31 10", "FDA7 AD", "FDAA 10"]

def keep(x):
	x = x.strip()
	for r in remove:
		if x.startswith(r):
			return False
	return True

sys.stderr.write("Reading trace A...\n")
traceA = open('nestopia_cpu.txt').readlines()
traceA = [x for x in traceA if keep(x)]
sys.stderr.write("Reading trace B...\n")
traceB = open('my_cpu.txt').readlines()
traceB = [x for x in traceB if keep(x)]

sys.stderr.write("Munging...\n")
n=0
failed = False
failcount = 0
while 1:
	if (n % 1000) == 0:
		sys.stderr.write("%d\n"%n)
	if n >= len(traceA):
		break
	if n >= len(traceB):
		break
	a = traceA[n].strip().upper()
	b = traceB[n].strip().upper()
	if not failed:
		if a != b:
			failed = True
			failcount = 5
	else:
		failcount += 1
		if failcount > 1000:
			break
	star = ''
	if a != b:
		star = ' ***'
	print '%s | %s%s' % (a, b, star)
	n += 1