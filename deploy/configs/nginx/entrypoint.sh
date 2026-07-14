#!/bin/sh
set -e

if [ -n "$OTEL_EXPORTER_OTLP_HEADERS" ]; then
    
    HEADER_NAME="${OTEL_EXPORTER_OTLP_HEADERS%%=*}"
    
    HEADER_VALUE="${OTEL_EXPORTER_OTLP_HEADERS#*=}"
    
    HEADER_VALUE=$(echo "$HEADER_VALUE" | sed 's/%20/ /g')
    
    AUTH_TYPE="${HEADER_VALUE%% *}"
    AUTH_TOKEN="${HEADER_VALUE#* }"

    if [ "$HEADER_NAME" = "Authorization" ] && [ "$AUTH_TYPE" = "Bearer" ] && [ -n "$AUTH_TOKEN" ]; then
        export NGINX_OTEL_HEADER="header Authorization \"Bearer ${AUTH_TOKEN}\";"
    else
        echo "ERROR: Invalid OTEL_EXPORTER_OTLP_HEADERS format." >&2
        echo "Expected 'Authorization=Bearer%20token', got '$OTEL_EXPORTER_OTLP_HEADERS'" >&2
        exit 1
    fi
else
    export NGINX_OTEL_HEADER=""
fi

envsubst '${OTEL_EXPORTER_OTLP_ENDPOINT} ${OTEL_EXPORTER_OTLP_PROTOCOL} ${NGINX_OTEL_HEADER}' \
    < /etc/nginx/nginx.conf.template \
    > /etc/nginx/nginx.conf

exec "$@"
