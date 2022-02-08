FROM envoyproxy/envoy-dev:fe17c1c95e26fbd34266b7a8c1f1acd7d78ba41a

COPY ./ssl/ /etc/ssl
COPY ./envoy.yaml /etc/envoy/envoy.yaml
COPY custom_entrypoint.sh /custom_entrypoint.sh
RUN chmod +x /custom_entrypoint.sh

ENTRYPOINT [ "/custom_entrypoint.sh" ]