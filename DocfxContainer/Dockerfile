FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY ./_site .
RUN rm 50x.html
RUN rm index.html
RUN ln -s ./api/RoomBooking.html ./index.html
EXPOSE 80