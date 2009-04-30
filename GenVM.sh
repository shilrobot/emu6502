#!/bin/sh
PYTHON=/cygdrive/c/python26/python.exe

$PYTHON GenVM.py && \
gcc -E vm.c | grep -v "#" | grep -v "^$" | sed 's/;;/;/g' > vm.cs && \
$PYTHON IntegrateVM.py