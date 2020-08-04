# NX.Node - A static (maybe) web site

Including the **Route.UI** DLL, makes the bee into a "static" web server.  Let's
look at the code:
```JavaScript
using System.Collections.Generic;

using NX.Engine;
using NX.Shared;

namespace Route.System
{
    /// <summary>
    ///
    /// A route that allows a "regular" website support
    ///
    /// Make sure that all of the files to be served are in the #rootfolder#/ui
    /// folder and that none of the subdirectories match a defined route
    ///
    /// </summary>
    public class UI : RouteClass
    {
        public override List<string> RouteTree => new List<string>() { RouteClass.GET, "?path?" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Assure folder
            call.Env.UIFolder.AssurePath();

            // Get the full path
            string sPath = store.PathFromEntry(call.Env.UIFolder, "path");

            // If not a file, then try using index.html
            if (!sPath.FileExists()) sPath = sPath.CombinePath("index.html");

            // And deliver
            call.RespondWithUIFile(sPath);
        }
    }
}
```
The key is the **RouteTree**, which has no text to match, just optional.
What this causes is to create a route where if a GET request finds no matches
elsewhere, it will match whatever the requestor entered.

If what was entered does not exist as a file, **index.html** is appended.

The root path is **ui_folder** and the file must be in that folder or a child folder.
If the file is not found a 404 Not found error is returned.

And I say maybe a static web system, as I can see where with a bit of code, you can
make this route into a processor and modify the files as they are being returned.

The config for this application looks like:
```JSON
{
    "uses": [ "Route.UI" ],
    "ui_folder": "@/etc/ui",

    // How many bees we want to have

    "qd_worker": [ "4" ],
    "qd_bumble": [ "traefik" ]
}
```

[Back to top](../README.md)