from Common import replaceRegion


fsrc = open("Chip6502_Tick.cs","rt")
fileContents = fsrc.read()
fsrc.close()
switch = open("vm.cs","rt").read()
fileContents = replaceRegion(fileContents, "SWITCH", "\n"+switch+"\n")

fdest = open("Chip6502_Tick.cs","wt")
fdest.write(fileContents)
fdest.close()