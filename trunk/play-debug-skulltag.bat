@echo off
if exist "temp\RGH-debug-skulltag.pk3" (
    skulltag temp/RGH-debug-skulltag.pk3
) else (
    echo "PK3 not found, build the mod first."
    pause
)
