FROM {repo_project}/{hive}:base
LABEL {proj_label}

RUN set -xe \
  && apt-get update \
  && apt-get install -y --no-install-recommends cron tini certbot jq \
  && update-rc.d cron disable \
  # cleanup
  && apt-get autoremove \
  && apt-get clean \
  && echo "" > /var/log/dpkg.log \
  && rm -rf /var/lib/apt/lists/*

COPY ./nxp /etc/nxp

VOLUME /certs

ENTRYPOINT ["tini", "--", "bash", "/etc/nxp/run/start"]