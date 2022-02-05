FROM node:12-bullseye as build

WORKDIR /usr/local/app
COPY . /usr/local/app/

RUN npm install --no-optional
ARG CONFIGURATION=production
COPY *secrets/environment.prod.ts /usr/local/app/src/environments

RUN npx ng build -c $CONFIGURATION


FROM nginx:latest as final

COPY nginx.conf /etc/nginx/nginx.conf
COPY secrets/ssl_certs /usr/share/nginx/ssl_certs
COPY --from=build /usr/local/app/dist/front /usr/share/nginx/html

EXPOSE 80 443