FROM {repo_project}/{hive}:base
LABEL {proj_label}

RUN set -xe \
  && apt-get update \
  && apt-get install -y tini nginx \
  && update-rc.d nginx disable \
  # cleanup
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* \
  # forward request and error logs to docker log collector
  && ln -sf /dev/stdout /var/log/nginx/access.log \
  && ln -sf /dev/stderr /var/log/nginx/error.log
ENTRYPOINT ["tini", "--", "nginx", "-g", "daemon off;"]