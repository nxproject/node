FROM {repo_project}/{hive}:base
LABEL {proj_label}

RUN set -xe \
  && apt-get update \
  && apt-get install -y tini redis-server \
  && update-rc.d redis-server disable \
  # cleanup
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["tini", "--", "redis-server"]