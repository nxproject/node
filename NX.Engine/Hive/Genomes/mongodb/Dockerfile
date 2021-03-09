# NXProject MongoDB 4.4 Image

FROM {repo_project}/base:{tier}
LABEL {proj_label}

ENV GOSU_VERSION="1.7"

COPY docker-entrypoint.sh /usr/local/bin/

RUN set -ex \
  && apt-get update \
  && apt-get install -y apt-transport-https ca-certificates jq numactl gnupg2 wget \
  && groupadd -r mongodb \
  && useradd -r -g mongodb mongodb \
  && wget -qO - https://www.mongodb.org/static/pgp/server-4.4.asc | apt-key add - \
  && echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu focal/mongodb-org/4.4 multiverse" | tee /etc/apt/sources.list.d/mongodb-org-4.4.list \
  && apt-get update \
  && wget -O /usr/local/bin/gosu "https://github.com/tianon/gosu/releases/download/$GOSU_VERSION/gosu-$(dpkg --print-architecture)" \
  && chmod +x /usr/local/bin/gosu \
  && gosu nobody true \
  # entrypoint
  && mkdir /docker-entrypoint-initdb.d \
  # MongoDB
  && apt-get install -y mongodb-org \
  && rm -rf /var/lib/mongodb \
  && mv /etc/mongod.conf /etc/mongod.conf.orig \
  && mkdir -p /data/db /data/configdb \
  && chown -R mongodb:mongodb /data/db /data/configdb \
  && ln -s usr/local/bin/docker-entrypoint.sh /entrypoint.sh \
  # cleanup
  && apt-get autoremove \
  && apt-get purge -y --auto-remove wget gnupg2 apt-transport-https ca-certificates \
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["docker-entrypoint.sh"]

EXPOSE 27017
CMD ["mongod"]