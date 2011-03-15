@echo off
@echo off
mkdir temp
mkdir temp\acs
mkdir temp\acs\acs
mkdir output
utility\mcpp "ACS source\RGH_ACS.acs" -o temp\acs\processed.acs -D SKULLTAG -D IgnoreHash(x)=x -P
echo ==============================================================
utility\acc temp\acs\processed.acs temp\acs\acs\RGH_ACS

if exist "temp\acs\acs.err" goto acserror

move temp\acs\acs\RGH_ACS.o temp\acs\acs\RGH_ACS

cd data
..\utility\7z u ..\temp\RGH-debug-skulltag.pk3 * -xr!.svn  -mx0
cd ..\temp\acs
..\..\utility\7z u ..\..\temp\RGH-debug-skulltag.pk3 acs\RGH_ACS -mx0
cd ..\..
skulltag temp/RGH-debug-skulltag.pk3
exit

:acserror
	echo ==============================================================
	move temp\acs\acs.err acs_errors.log	
	pause
