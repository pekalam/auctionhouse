# ====== Dev/Test - build with node docker image
FROM node:12-bullseye as build

WORKDIR /usr/local/app
COPY . /usr/local/app/

RUN npm install --no-optional
ARG CONFIGURATION=production
COPY *secrets/environment.prod.ts /usr/local/app/src/environments

RUN npx ng build -c $CONFIGURATION

# target for hosting application built with docker
FROM nginx:latest as local

COPY nginx.conf /etc/nginx/nginx.conf
COPY secrets/ssl_certs /usr/share/nginx/ssl_certs
COPY --from=build /usr/local/app/dist/front /usr/share/nginx/html
#prevent 404 errors
RUN mkdir /usr/share/nginx/html/front_config && touch /usr/share/nginx/html/front_config/config.js

EXPOSE 80 443
COPY custom_entrypoint.sh /custom_entrypoint.sh
RUN chmod +x /custom_entrypoint.sh
# ======

# ====== hosting already built angular application
FROM nginx:latest as hosting

COPY nginx.conf /etc/nginx/nginx.conf
COPY secrets/ssl_certs /usr/share/nginx/ssl_certs
COPY dist/front /usr/share/nginx/html
#prevent 404 errors
RUN mkdir /usr/share/nginx/html/front_config && touch /usr/share/nginx/html/front_config/config.js

EXPOSE 80 443
COPY custom_entrypoint.sh /custom_entrypoint.sh
RUN chmod +x /custom_entrypoint.sh

ENTRYPOINT [ "/custom_entrypoint.sh" ]
CMD [ "nginx", "-g", "daemon off;" ]
# ======