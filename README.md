# NX.Node

Have you wondered how to have your C# code run inside NodeJS?  You have probably
tried Edge,JS or running your code as a command line process.  I did.  Found the
first to be a bit unreliable and the second to be a bit cumbersome, so I said to
myself, Isn't the probem that NodeJS, which is a great product, is JavaScript? So
what would happen if instead I had a NodeJS plus ExpressJS in C# instead.

So here we are.

## The best way to learn

The best to learn what the code does is and always will be by looking at the code
itself.  *NX.Node*  is a .NET Code 3.1 C# Visual Studio project, so download the
code, open it with Visual Studio (use **Visual Studio Community 2019** or later,
it's free) and get ready to view the code.  using MS Windows 10 or later, you can
easily install [Docker Desktop on Windows](https://docs.docker.com/docker-for-windows/install/).

* [Routes](docs/README_ROUTES.md)
* [Functions](docs/README_FNS.md)
* [Processes](docs/README_PROCS.md)
* [Loading processes, routes and functions](docs/README_USE.md)
* [Environment](docs/README_ENV.md)

## Containers are bees

As **NX.Node** is designed to run as a Docker container, a set of classes are
part of the **NX.Engine** to supports containers.  In the NX universe, a container
is called a **bee**, which live in a **hive** and can transverse many **fields**.
Let's dive into how they relate.

Term|Meaning
----|-------
hive|Logical entity, like a company or department.
field|IP address a Docker deamon.
roster|A list of all the bees in the hive.
bee|A Docker container which is part of a hive.  It can be born in any one of the fields.
worker bee|A bee carries out work.
ghost bee|A bee that is outside all the fields.  Typically the bee that gets created when debugging.
bumble bee|A bee that provides services.
cv|Once born, each bee has a CV, which looks a lot like the Docker definition for
ListContainers, with a few extra values.
ticklearea|Each bee can have any number of tickle areas, which are exposed ports
genome|A Dockerfile
DNA|All bees have a DNA definition when they are being created.  DNA looks like
the information passed to the Docker CreateContainer call with a few extra values,
but a shorthand definition is available.

And best explained in this sequence:

* [Genome](docs/README_B_GENOME.md)
* [DNA](docs/README_B_DNA.md)
* [Hive](docs/README_B_HIVE.md)
* [Fields](docs/README_B_FIELD.md)
* [Roster](docs/README_B_ROSTER.md)
* Bees
	* [Worker Bee](docs/README_B_BEE.md)
	* [Queen Bee](docs/README_B_QUEEN.md)
	* [Bumble bee](docs/README_B_BUMBLE.md)

## Other subjects
* [DotNet](docs/README_DOTNET.md)
* [Tiers](docs/README_TIER.md)
* [Git](docs/README_GIT.md)
* [A static (maybe) web site](docs/README_STATIC.md)
* [HTTP](docs/README_HTTP.md)
* [XML](docs/README_XML.md)
* [Kubernetes](docs/README_KUBE.md)
* [Where do I put my files](docs/README_FOLDERS.md)
* [How do I debug my code?](docs/README_DEBUG.md)

## Prerequisites

This project was generated as a C# .NET Core 3.1 project, soo you will need Visual
Studio Community 2019 or later.

If you do not wish to provide field IP definitions and having just one field, your
field must be running Docker V18.03 or later.

Make sure that any firewall that you are using allows for full access to the ports
being used by Docker.  I just turn off protection for my private network.

## Author

* **Jose E. Gonzalez jr.** - *All that you see here*

### A trivia question

The DNA for a worker bee is **processor**.  Why?

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details

## Packages used

* [BouncyCastle](http://www.bouncycastle.org/csharp/)
* [Docker.DotNet](https://github.com/dotnet/Docker.DotNet)
* [DocumentFormat.OpenXML](https://github.com/OfficeDev/Open-Xml-Sdk)
* [HtmlAgilityPack](https://github.com/zzzprojects/html-agility-pack)
* [ICSharpCode.SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
* [iTextSharp](https://github.com/itext/itextsharp)
* [itextSharp.xmlworker](https://github.com/itext/itextsharp)
* [Jint](https://github.com/sebastienros/jint)
* [Minio](https://github.com/minio/minio)
* [MongoDB.Bson](http://mongodb.github.io/mongo-java-driver/3.0/bson/)
* [MongoDB.Driver](https://github.com/mongodb/mongo-csharp-driver)
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
* [Octokit](https://github.com/octokit)
* [OpenXmlPowerTools](https://github.com/OfficeDev/Open-Xml-PowerTools)
* [SocketIOClient](https://github.com/socketio/socket.io-client)
* [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis)
* [TimeZoneConverter](https://github.com/mj1856/TimeZoneConverter)

## Why

This code represents many years of coding in many languages, operating systems computer
types and places.  It is intended to be an example of open source as what I wish
Open Source to be, with an MIT license.

**Take from the code what you wish.**

And to avoid any "style" wars, the code follows my own coding style, which does not
mean that your coding style is any worse, it just means that we differ.  The same goes
for language, operating system and  whatever else you care about.  I am glad that
you care about it, but I do not care to get into any arguments.  Like a colleague
once said **Let this be a learning moment for you, not a teaching moment for me**.

Do bear with me, this is my first open source project, based on years of work 
in many languages. I know that my conversion work is not done, until then you will 
see a version number of less than 1.0. And like any programmer, the code will
change to adapt to new things.

NX.Node is one piece of a multi-part project, so expect changes as I work on the other layers!

## My take on containers

The computer world is and has been in a cycle.  I begun with a "mainframe" that could be
used by one person at a time, in other words a "personal" computer, just a very big
"personal" computer.  Time-sharing came into play, as the big, expensive computer could
then be used by many people at the "same" time.  Each computer sice became the personal computer 
in the 80's, which begun with the same coputing power of the first mainframes that I  used,
it just sat on your desk.  Over time, the power of that computer grew and by linking them
together, we got "the cloud", one really big computer, but made of "litte" computers
which could "timeshare" the respurces by using containers.

I like "the cloud", but not just to run a billion people on one "application", I want
to run a different applications for a billion people.  A hive is the platform for such an application.

Imagine going to your dog walker and being able to setup a fairly complex application that
handles everything from sending out "flyers" to back room accounting, with video conferencing
and text messages thrown in.  But just for him/her.  And then doing the same, with a bit
of customization, for the sandwich shop at the corner.  And just for them.

That is what my goal is, and has been since 1983.

## Acknowledgments

* To all of those that came before me
* To my eldest son **Joemar**, for whom I wish this to be the starting place of something good.  **Alice** is two years older than you are.  Thank you for your contribution of all the Genomes and DNA.
* To Lewis Carroll and his wonderful **Alice's Adventure in Wonderland**, after which the original project that ended here was named, **Alice**
* To the people at **Microsoft** for creating the often painful, but overall useful Visual Studio .NET, which has been my home since it was first released, and the creation of .NET to which I was introduced early in 2001 and have been toiling at ever since.
* To **Herre Kuijpers** for his [a Tiny Parser Generator v1.2](https://www.codeproject.com/Articles/28294/a-Tiny-Parser-Generator-v1-2) which has been hacked to death in so many incarnations.  It is the only non-package in the system.
* To the people at **Atom** https://atom.io/ which created the nifty editor, the results of which you just read
* To [Gary Larson](https://www.thefarside.com/2020/08/05/2), always kept me smiling
* To [Tom Toles](https://en.wikipedia.org/wiki/Tom_Toles), will miss your witty work!