# NXProject Traefik 1.7.9 Image

FROM {repo_project}/base:{tier}
LABEL {proj_label}

RUN set -ex; \
    && ARCH= && dpkgArch="$(dpkg --print-architecture)" \
    && case "${dpkgArch##*-}" in \
    amd64) ARCH='amd64';; \
    ppc64el) ARCH='ppc64le';; \
    arm64) ARCH='arm64';; \
    i386) ARCH='386';; \
    *) echo "unsupported architecture"; exit 1 ;; \
    esac \
	&& curl --quiet -o /usr/local/bin/traefik "https://github.com/containous/traefik/releases/download/v1.7.9/traefik_linux-$ARCH"; \
	&& chmod +x /usr/local/bin/traefik
COPY entrypoint.sh /
EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
CMD ["traefik"]
