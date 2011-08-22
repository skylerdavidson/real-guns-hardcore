@echo off
if exist "output\RGH-Skulltag.pk3" del output\RGH-Skulltag.pk3
if exist "output\RGH-Skulltag.pk3" del output\RGH-ZDoom.pk3
if not exist "temp" mkdir temp
if not exist "temp\acs" mkdir temp\acs
if not exist "temp\acs-skulltag\acs" mkdir temp\acs-skulltag\acs
if not exist "temp\acs-zdoom\acs" mkdir temp\acs-zdoom\acs
if not exist "output" mkdir output
echo ==============================================================
utility\mcpp "ACS source\RGH_ACS.acs" -o temp\acs-skulltag\processed.acs -D SKULLTAG -D IgnoreHash(x)=x -P
utility\acc temp\acs-skulltag\processed.acs temp\acs-skulltag\acs\RGH_ACS
if exist "temp\acs-skulltag\acs.err" (
    echo ==============================================================
    move /y temp\acs-skulltag\acs.err acs_errors_skulltag.log
    pause
) else (
    echo ==============================================================
    utility\mcpp "ACS source\RGH_ACS.acs" -o temp\acs-zdoom\processed.acs -D ZDOOM -D IgnoreHash(x)=x -P
    utility\acc temp\acs-zdoom\processed.acs temp\acs-zdoom\acs\RGH_ACS
    if exist "temp\acs-zdoom\acs.err" (
        echo ==============================================================
        move /y temp\acs-zdoom\acs.err acs_errors_zdoom.log	
        pause
    ) else (
        move /y temp\acs-skulltag\acs\RGH_ACS.o temp\acs-skulltag\acs\RGH_ACS
        move /y temp\acs-zdoom\acs\RGH_ACS.o temp\acs-zdoom\acs\RGH_ACS

        cd data
        ..\utility\7z a ..\output\RGH-Skulltag.pk3 * -xr!.svn -mx0
        copy ..\output\RGH-Skulltag.pk3 ..\output\RGH-ZDoom.pk3
        cd ..\temp\acs-skulltag
        ..\..\utility\7z a ..\..\output\RGH-Skulltag.pk3 acs\RGH_ACS -mx0
        cd ..\acs-zdoom
        ..\..\utility\7z a ..\..\output\RGH-ZDoom.pk3 acs\RGH_ACS -mx0
        cd "..\..\zdoom data"
        ..\utility\7z u ..\output\RGH-ZDoom.pk3 * -xr!.svn -mx0
        cd ..
        exit
    )
)
