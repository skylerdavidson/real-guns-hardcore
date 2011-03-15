You can use one of the build-debug-skulltag, build-debug-zdoom or build-release batch files to build the mod.

Use the debug build files while you are doing any modification to the mod files. Unlike release build, debug build is incremental (only the modified files are rebuilt), so after the initial build, incremental builds are extremely fast.

Once the build is complete (the console window closes), you can find the resulting PK3 file in either output directory (if you used release build script) or temp (if you used debug). You can now play with this file as if it was an official release.

You can now also update the directory to latest version by right clicking the directory and clicking the "SVN Update". When the window closes, all the files will be 100% up-to-date! Now just rebuild the PK3 and you have the lastest version without downloading whole 80+ megabyte package :)

One small remark regarding the release build scripts: If you (for some reason) remove a file from the source tree, you have to manually delete the PK3 file from the temp directory and rebuild the file from scratch. The incremental build won't recognize that a file was deleted and would not be removed from the PK3.