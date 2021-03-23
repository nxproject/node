# NXProject Traefik 1.7.9 Image

FROM {repo_project}/{hive}:base
LABEL {proj_label}

RUN set -ex; \
curl -o /usr/local/bin/traefik "https://github.com/containous/traefik/releases/download/v2.3.6/traefik_linux-amd64"; \
chmod +x /usr/local/bin/traefik
COPY entrypoint.sh /
EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
CMD ["traefik"]