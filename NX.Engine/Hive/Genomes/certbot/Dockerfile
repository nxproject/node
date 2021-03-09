FROM {repo_project}/base:{tier}
LABEL {proj_label}

RUN set -xe \
  && apt-get update \
  && apt-get install -y cron tini certbot jq \
  && update-rc.d cron disable \
  # cleanup
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

COPY ./run /run

VOLUME /certs

ENTRYPOINT ["tini", "--", "bash", "/run/start"]