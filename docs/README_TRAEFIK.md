# NX.Node - Traefik

The traefik genome creates a Traefik instance.  As Traefik is used as a front-end for
multiple hives, the **hive_traefik** environment settings, as explained in
[Bumble bee](README_B_BUMBLE.md) section **Sharing bumble bees** is used to determine
which hives are to be included.  

The first hive defined is the one that will host the traefik bumble bee.

You tell the system that you want to use Traefik by using:
```
--qd_uses Proc.TraefiK
```

## Include all the hives

As the number of hives may be dynamic, there is one more option available:
```
--hive_traefik hivetohost --hive_traefik *
```
The first entry tells the system which hive will host the traefik bumble bee and the 
second tells the system that all hives that have this entry will be part of the routing.

## SSL

NX.Node uses [Let's Encrypt](https://letsencrypt.org/) to get the SSL certificates needed
for HTTP support.  In order to do this you need to define the three environment settings:

Setting|Meaning
-------|-------
traefik_domain|The domain to be used (eg. mydomain.com)
traefik_email|An email address where notifications will be sent
traefik_provider|The domain provider name (Default: namecheap)

You need to setup the DNS for the domain per [these instructions](https://letsencrypt.org/how-it-works/).

## Accessing each hive

Once you have your domain setup, each hive becomes a sub-domain, so to access hive **finance**
you would call:
```
https://finance.mydomain.com/....
```
 
[Back to top](../README.md)