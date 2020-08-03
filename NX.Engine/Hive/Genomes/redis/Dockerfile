# NXProject Redis 4.0.2 Image

FROM {repo_project}/base:{tier}
LABEL {proj_label}

ENV GOSU_VERSION="1.10" \
  REDIS_VERSION="4.0.2" \
  REDIS_DOWNLOAD_URL="http://download.redis.io/releases/redis-4.0.2.tar.gz" \
  REDIS_DOWNLOAD_SHA="b1a0915dbc91b979d06df1977fe594c3fa9b189f1f3d38743a2948c9f7634813"
COPY docker-entrypoint.sh /usr/local/bin/
RUN set -ex \
  && groupadd -r redis && useradd -r -g redis redis \
  && fetchDeps="ca-certificates wget gcc libc6-dev make gpg dirmngr" \
  && apt-get update \
  && apt-get install -yq $fetchDeps \
  #
  # && mkdir ~/.gnupg \
  # && echo "disable-ipv6" >> ~/.gnupg/dirmngr.conf \
  # GOSU
  && dpkgArch="$(dpkg --print-architecture | awk -F- '{ print $NF }')" \
  # && gpg --keyserver ha.pool.sks-keyservers.net --recv-keys B42F6819007F00F88E364FD4036A9C25BF357DD4 \
  && wget -O /usr/local/bin/gosu "https://github.com/tianon/gosu/releases/download/$GOSU_VERSION/gosu-$dpkgArch" \
  && wget -O /usr/local/bin/gosu.asc "https://github.com/tianon/gosu/releases/download/$GOSU_VERSION/gosu-$dpkgArch.asc" \
  # && export GNUPGHOME="$(mktemp -d)" \
  # && gpg --batch --verify /usr/local/bin/gosu.asc /usr/local/bin/gosu \
  # && rm -r "$GNUPGHOME" /usr/local/bin/gosu.asc \
  && chmod +x /usr/local/bin/gosu \
  && gosu nobody true \
  # redis
  && wget -O redis.tar.gz "$REDIS_DOWNLOAD_URL" \
  && echo "$REDIS_DOWNLOAD_SHA *redis.tar.gz" | sha256sum -c - \
  && mkdir -p /usr/src/redis \
  && tar -xzf redis.tar.gz -C /usr/src/redis --strip-components=1 \
  && rm redis.tar.gz \
  && grep -q '^#define CONFIG_DEFAULT_PROTECTED_MODE 1$' /usr/src/redis/src/server.h \
  && sed -ri 's!^(#define CONFIG_DEFAULT_PROTECTED_MODE) 1$!\1 0!' /usr/src/redis/src/server.h \
  && grep -q '^#define CONFIG_DEFAULT_PROTECTED_MODE 0$' /usr/src/redis/src/server.h \
  && make -C /usr/src/redis -j "$(nproc)" \
  && make -C /usr/src/redis install \
  && rm -r /usr/src/redis \
  && mkdir /data && chown redis:redis /data \
  # cleanup
  && apt-get -yq remove $fetchDeps \
  && apt-get purge -y --auto-remove $fetchDeps \
  && apt-get -yq autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*
WORKDIR /data
ENTRYPOINT ["docker-entrypoint.sh"]
EXPOSE 6379
CMD ["redis-server"]
