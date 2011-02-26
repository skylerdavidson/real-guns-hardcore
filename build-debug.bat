@echo off
del output\RGH.pk3
mkdir temp
mkdir temp\acs
mkdir temp\acs\acs
mkdir output
utility\acc "ACS source\RGH_ACS.acs" temp\acs\acs\RGH_ACS

move temp\acs\acs\RGH_ACS.o temp\acs\acs\RGH_ACS

if exist "acs.err" goto acserror

cd data\monsters
..\..\utility\7z u ..\..\temp\debug-monsters.pk3 *.wad -mx0
cd ..
..\utility\7z a ..\temp\debug-acs.pk3 LOADACS.txt -mx0
..\utility\7z u ..\temp\debug-data.pk3 sprites\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 sprites\*\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 sprites\*\*\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*.wav -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*.ogg -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*.mp3 -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*\*.wav -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*\*.ogg -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*\*.mp3 -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*\*\*.wav -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*\*\*.ogg -mx0
..\utility\7z u ..\temp\debug-data.pk3 sounds\*\*\*.mp3 -mx0
..\utility\7z u ..\temp\debug-data.pk3 graphics\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 graphics\*\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 graphics\*\*\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 hires\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 hires\*\*.png -mx0
..\utility\7z u ..\temp\debug-data.pk3 hires\*\*\*.png -mx0
cd ..\temp\acs
..\..\utility\7z a ..\..\temp\debug-acs.pk3 acs\RGH_ACS -mx0
cd ..\..
skulltag data/ temp/debug-data.pk3 data/core/*.wad temp/debug-acs.pk3 temp/debug-monsters.pk3
exit

:acserror
	echo ==============================================================
	type acs.err
	move acs.err acs_err.log
	echo ==============================================================
	del acs.err
	pause
