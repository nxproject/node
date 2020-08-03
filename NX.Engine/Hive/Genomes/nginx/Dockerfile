# NXProject NGINX 1.16.0 Image

FROM {repo_project}/base:{tier}
LABEL {proj_label}

ENV NGINX_VERSION="1.16.0-1~bionic" \
  NJS_VERSION="1.16.0.0.3.1-1~bionic"

RUN set -x \
  && nginxPackages="nginx=${NGINX_VERSION} nginx-module-xslt=${NGINX_VERSION} nginx-module-geoip=${NGINX_VERSION} nginx-module-image-filter=${NGINX_VERSION} nginx-module-njs=${NJS_VERSION}" \
  && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv ABF5BD827BD9BF62 \
  && echo "deb http://nginx.org/packages/ubuntu/ bionic nginx" > /etc/apt/sources.list.d/nginx.list \
  && echo "deb-src http://nginx.org/packages/ubuntu/ bionic nginx" >> /etc/apt/sources.list.d/nginx.list \
  && apt-get update \
  && apt-get install -y $nginxPackages gettext-base \
  # cleanup
  && apt-get autoremove \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/* \
  # forward request and error logs to docker log collector
  && ln -sf /dev/stdout /var/log/nginx/access.log \
  && ln -sf /dev/stderr /var/log/nginx/error.log

EXPOSE 80

STOPSIGNAL SIGTERM

CMD ["nginx", "-g", "daemon off;"]
