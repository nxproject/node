# NXProject MongoDB 3.4.10 Image

FROM {repo_project}/base:{hive}
LABEL {proj_label}

ENV GOSU_VERSION="1.7" \
  MONGO_PACKAGE="mongodb-org" \
  MONGO_REPO="repo.mongodb.org" \
  MONGO_MAJOR="3.4" \
  MONGO_VERSION="3.4.10"

COPY docker-entrypoint.sh /usr/local/bin/

RUN set -ex \
  && apt-get update \
  && apt-get install -y apt-transport-https ca-certificates jq numactl \
  && groupadd -r mongodb \
  && useradd -r -g mongodb mongodb \
  && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 0C49F3730359A14518585931BC711F9BA15703C6 \
  && echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu xenial/mongodb-org/$MONGO_MAJOR multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-${MONGO_MAJOR}.list \
  && apt-get update \
  # GOSU
  && wget -O /usr/local/bin/gosu "https://github.com/tianon/gosu/releases/download/$GOSU_VERSION/gosu-$(dpkg --print-architecture)" \
  && chmod +x /usr/local/bin/gosu \
  && gosu nobody true \
  # entrypoint
  && mkdir /docker-entrypoint-initdb.d \
  # MongoDB
  && apt-get install -y ${MONGO_PACKAGE}=$MONGO_VERSION ${MONGO_PACKAGE}-server=$MONGO_VERSION ${MONGO_PACKAGE}-shell=$MONGO_VERSION ${MONGO_PACKAGE}-mongos=$MONGO_VERSION ${MONGO_PACKAGE}-tools=$MONGO_VERSION \
  && rm -rf /var/lib/mongodb \
  && mv /etc/mongod.conf /etc/mongod.conf.orig \
  && mkdir -p /data/db /data/configdb \
  && chown -R mongodb:mongodb /data/db /data/configdb \
  && ln -s usr/local/bin/docker-entrypoint.sh /entrypoint.sh \
  # cleanup
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["docker-entrypoint.sh"]

EXPOSE 27017
CMD ["mongod"]
