FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN ls -l publish/

# Entrypoint that uses mounted artifacts
ENTRYPOINT ["dotnet", "publish/RoomBooking.dll"]