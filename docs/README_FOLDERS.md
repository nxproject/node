# NX.Node - Where do I put my files

The folder structure look like this:

   ^				Root folder
   |
   +-------X		Dynamic folder
   |
   +-------X		Shared folder
		   |
		   +----X	Document folder
		   |
		   +----X	UI folder


These are the environment settings used by the folder mapping:

Setting|Meaning|Default
-------|-------|-------
root_folder|Starting point of the folder structure|Working directory
dyn_folder|Folder where loaded DLLs are kept|#root_folder#/dyn
shared_folder|Folder where the shared items are kept|#root_folder#/shared
doc_folder|Folder where documents are kept|#shared_folder#/files
ui_folder|Folder where UI files are kept|#shared_folder#/ui

You can change the subfolder by setting the environment setting as follows:
```
--ui_folder web
```
which would produce the folder to be the value of **share_folder** with **web** appended
as a sub folder.

If you want to set the full path, simply enter the path as follows:
```
--ui_folder @/etc/web
```

The one setting that you may want to change to a full path is the UI folder, which
is only used by the [A static (maybe) web site](README_STATIC.md)

## Linux v. MS Windows

The bees themselves all live in Linux, and you must think in Linux.  Folder paths
are in the format of **/folder/subfolder1/subfoldeer2...**.  It applies to the following:

* config
* genome_source
* xxx_folder

The config and genome_source are not stored in the shared environment settings.

[Back to top](../README.md)