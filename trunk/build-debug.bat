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
..\utility\7z u ..\temp\RGH-debug.pk3 * -xr!.svn  -mx0
cd ..\temp\acs
..\..\utility\7z u ..\..\temp\RGH-debug.pk3 acs\RGH_ACS -mx0
cd ..\..
skulltag temp/RGH-debug.pk3
exit

:acserror
	echo ==============================================================
	type acs.err
	move acs.err acs_err.log
	echo ==============================================================
	del acs.err
	pause
