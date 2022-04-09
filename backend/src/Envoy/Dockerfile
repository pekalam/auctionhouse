FROM envoyproxy/envoy:v1.21.1

COPY ./envoy.yaml /etc/envoy/envoy.yaml
COPY custom_entrypoint.sh /custom_entrypoint.sh
RUN chmod +x /custom_entrypoint.sh

ENTRYPOINT [ "/custom_entrypoint.sh" ]