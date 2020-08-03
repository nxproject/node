# NXProject Percona 4.0.10-5.bionic Image

FROM {repo_project}/base:{tier}
LABEL {proj_label}

ENV PERCONA_MAJOR="40" \
  PERCONA_VERSION="4.0.10-5.bionic"
COPY docker-entrypoint.sh /entrypoint.sh
RUN set -x \
  && useradd -u 1001 -r -g 0 -s /sbin/nologin -c "Default Application User" mongodb \
  && apt-get update \
  && apt-get install -y --no-install-recommends apt-transport-https ca-certificates pwgen wget gpg lsb-release gosu \
  && rm -rf /var/lib/apt/lists/* \
  && RELEASE=$(lsb_release -sc) \
  && wget https://repo.percona.com/apt/percona-release_latest.${RELEASE}_all.deb \
  && dpkg -i percona-release_latest.${RELEASE}_all.deb \
  && rm percona-release_latest.${RELEASE}_all.deb \
  && percona-release enable psmdb-40 release \
  && apt-get update \
  && apt-get  -yq install --force-yes percona-server-mongodb=$PERCONA_VERSION \
  && apt-get -yq remove apt-transport-https ca-certificates wget gpg lsb-release \
  && apt-get -yq autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* \
  && rm -rf /data/db \
  && mkdir -p /data/db \ 
  #&& chown -R 1001:0 /data/db \
  && mkdir -p /data/configdb \ 
  #&& chown -R 1001:0 /data/configdb \
  && chmod +x /entrypoint.sh \
  #&& chown -R 1001:0 /entrypoint.sh
VOLUME ["/data/db"]
ENTRYPOINT ["bash", "/entrypoint.sh"]
EXPOSE 27017
CMD ["mongod"]
