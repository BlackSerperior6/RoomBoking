FROM nginx:alpine
WORKDIR /usr/share/nginx/html
RUN rm 50x.html
RUN rm index.html
COPY ./_site .
EXPOSE 80