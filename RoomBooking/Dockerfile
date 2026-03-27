FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Entrypoint that uses mounted artifacts
ENTRYPOINT ["dotnet", "/publish/RoomBooking.dll"]