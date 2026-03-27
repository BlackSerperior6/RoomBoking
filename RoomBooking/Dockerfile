FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY ./publish ./publish

# Entrypoint that uses mounted artifacts
ENTRYPOINT ["dotnet", "publish/RoomBooking.dll"]
#ENTRYPOINT ["/bin/bash"]
#CMD ["-c", "tail -f /dev/null"]