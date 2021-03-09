FROM {repo_project}/base:{tier}
LABEL {proj_label}

COPY entrypoint.sh /entrypoint.sh
RUN set -xe \
  && apt-get update \
  && apt-get install -y wget gnupg2 lsb-release curl \
  && wget https://repo.percona.com/apt/percona-release_latest.generic_all.deb \
  && dpkg -i percona-release_latest.generic_all.deb \
  && apt-get update \
  && apt-get install -y --fix-broken \
  && percona-release setup pdmdb-4.4 \
  && apt-get install -y tini percona-server-mongodb percona-backup-mongodb \
  && update-rc.d mongod disable \
  # mongo housekeeping
  # && useradd -u 1001 -r -g 0 -s /sbin/nologin -c "mongodb" mongodb \
  && mv /etc/mongod.conf /etc/mongod.conf.orig \
  && rm -rf /data/db \
  && mkdir -p /data/db \ 
  && mkdir -p /data/configdb \ 
  # && chown -R 1001:0 /data/db \
  # && chown -R 1001:0 /data/configdb \
  # cleanup
  && rm percona-release_latest.generic_all.deb \
  && apt-get autoremove \
  && apt-get purge -y --auto-remove wget gnupg2 lsb-release curl \
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["tini", "--", "bash", "/entrypoint.sh"]
CMD ["mongod"]