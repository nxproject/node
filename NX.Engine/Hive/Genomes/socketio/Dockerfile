## https://github.com/dmhendricks/docker-socketio-relay

FROM mhart/alpine-node:10
LABEL {proj_label}

COPY / /etc/wd/

WORKDIR /etc/wd

RUN yarn --prod && npm install
