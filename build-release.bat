
del output\RGH-Skulltag.pk3
del output\RGH-ZDoom.pk3
mkdir temp
mkdir temp\acs
mkdir temp\acs-skulltag\acs
mkdir temp\acs-zdoom\acs
mkdir output
utility\mcpp "ACS source\RGH_ACS.acs" -o temp\acs-skulltag\processed.acs -D SKULLTAG -D IgnoreHash(x)=x -P
utility\acc temp\acs-skulltag\processed.acs temp\acs-skulltag\acs\RGH_ACS

utility\mcpp "ACS source\RGH_ACS.acs" -o temp\acs-zdoom\processed.acs -D ZDOOM -D IgnoreHash(x)=x -P
utility\acc temp\acs-zdoom\processed.acs temp\acs-zdoom\acs\RGH_ACS

move temp\acs-skulltag\acs\RGH_ACS.o temp\acs-skulltag\acs\RGH_ACS
move temp\acs-zdoom\acs\RGH_ACS.o temp\acs-zdoom\acs\RGH_ACS

if exist "acs.err" goto acserror

cd data
..\utility\7z a ..\output\RGH-Skulltag.pk3 * -xr!.svn -tzip
copy ..\output\RGH-Skulltag.pk3 ..\output\RGH-ZDoom.pk3
cd ..\temp\acs-skulltag
..\..\utility\7z a ..\..\output\RGH-Skulltag.pk3 acs\RGH_ACS -tzip
cd ..\acs-zdoom
..\..\utility\7z a ..\..\output\RGH-ZDoom.pk3 acs\RGH_ACS -tzip
cd "..\..\zdoom data"
..\utility\7z u ..\output\RGH-ZDoom.pk3 * -xr!.svn -tzip
cd ..
exit

:acserror
	echo ==============================================================
	type acs.err
	move acs.err acs_err.log
	echo ==============================================================
	del acs.err
	pause
