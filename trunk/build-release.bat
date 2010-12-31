@echo off
del output\RGH.pk3
mkdir temp
mkdir temp\acs
mkdir temp\acs\acs
mkdir output
utility\acc "ACS source\RGH_ACS.acs" temp\acs\acs\RGH_ACS

move temp\acs\acs\RGH_ACS.o temp\acs\acs\RGH_ACS

if exist "acs.err" goto acserror

cd data
..\utility\7z a ..\output\RGH.pk3 credits.txt
cd core
..\..\utility\7z a ..\..\output\RGH.pk3 *.wad
cd ..\monsters
..\..\utility\7z a ..\..\output\RGH.pk3 *.wad
cd ..
..\utility\7z a ..\output\RGH.pk3 LOADACS
cd ..\temp\acs
..\..\utility\7z a ..\..\output\RGH.pk3 acs\RGH_ACS
cd ..\..
exit

:acserror
	echo ==============================================================
	type acs.err
	move acs.err acs_err.log
	echo ==============================================================
	del acs.err
	pause
