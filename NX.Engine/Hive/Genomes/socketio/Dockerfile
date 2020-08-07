# https://raw.githubusercontent.com/voduytuan/docker-socketio/master/Dockerfile

# app.js: https://socket.io/docs/server-api/#new-Server-httpServer-options

FROM zzrot/alpine-node
MAINTAINER Vo Duy Tuan <tuanmaster2012@gmail.com>

RUN apk add --update \
    supervisor \
  && rm -rf /var/cache/apk/*

ADD supervisord.conf /etc/supervisor/conf.d/supervisord.conf
ADD app.js /etc/supervisor/app/app.js

RUN /bin/mkdir -p /srv/logs

WORKDIR /srv

RUN npm install --silent socket.io
RUN npm dedupe

EXPOSE 8080

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
