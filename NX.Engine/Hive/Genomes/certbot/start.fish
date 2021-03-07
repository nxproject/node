#!/usr/bin/env fish

if not string length $DOMAIN >/dev/null
    echo "DOMAIN must be set" >&2
    exit 1
end

if not test -d /certs
    echo "/certs is not a valid directory" >&2
    exit 1
end
if not test -f /certs/live/$DOMAIN/fullchain.pem 
    tini -- /run/renew-all
    #exec /run/renew-all
end

if test -n "$RUN_ONCE"; and test "$RUN_ONCE" = 'true'
    exec /run/renew-all
else
    exec crond -f -c /crontabs -L /dev/stderr
end
