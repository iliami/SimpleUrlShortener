FROM nginx:1.31.2-trixie-otel

RUN apt-get update && apt-get install -y --no-install-recommends \
    wget \
    sed \
    gettext-base \
    && rm -rf /var/lib/apt/lists/*

COPY index.html /etc/nginx/html/index.html
COPY nginx.conf.template /etc/nginx/nginx.conf.template
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
