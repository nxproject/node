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
        public override List<string> RouteTree => new List<string>() { RouteClass.GET(), "?path?" };
        public override void Call(HTTPCallClass call, StoreClass store)
        {
            // Make the folder path
            string sPath = call.Env.RootFolder.CombinePath("modulesui").CombinePath(call.Env.UI);

            // Get the full path
            sPath = store.PathFromEntry(sPath, "path");

            // Assure folder
            sPath.AssurePath();

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

If the resultant path is not found a 404 Not found error is returned.

And I say maybe a static web system, as I can see where with a bit of code, you can
make this route into a processor and modify the files as they are being returned.

The config for this application looks like:
```JSON
{
    "ui": "react"
}
```
which will use the React boilerplate.

The following choices are available:

Code|System
bootstrap|[Bootstrap](https://getbootstrap.com)
html|HTML based system
qx|[qooxdoo](https://qooxdoo.org/about.html)
react|[React](https://github.com/facebook/react)
vue|[Vue](https://vuejs.org)

## Adding your code

If you include your web site pages in the Visual Studio solution you can set the config
as follows:
```JSON
{
    "make_bee": "y",
    "ui": "react",
    "code_folder": "folderwithwebsite=ui"
}
```
The **ui** option at the end of the **code_folder** tells the system that the folder
contains the website UI.  This code will then be added to the proper boilerplate,
given by the **ui** environemnt setting.

## <nxjs>

You can include JavaScript in your .html pages by the use of **nxjs** tag.  For example:
```HTML
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8" />
    <title>Hello World!</title>
</head>
<body>
    <nxjs>
        html.Add(html.h2("Hello World!"), html.p("I am ", env.Hive.Name));
    </nxjs>
</body>
</html>
```
would produce the HTML page of:

``` ------------------------------- Top ----------------------------------```

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="utf-8">
    <title>Hello World!</title>
</head>
<body>
    <h2>Hello World!</h2><p>I am test</p>
</body>

``` ----------------------------- Bottom ---------------------------------```

The following are available in the JavaScript:

Variable|Meaning
--------|-------
env|The current environment
call|The HTTP call
html|HTML generator


[Back to top](../README.md)
