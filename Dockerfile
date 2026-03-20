# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта и восстанавливаем зависимости
COPY ["RoomBooking.csproj", "."]
RUN dotnet restore "RoomBooking.csproj"

# Копируем все исходники и публикуем приложение
COPY . .
RUN dotnet publish "RoomBooking.csproj" -c Release -o /app/publish

# Этап рантайма
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Копируем собранное приложение из этапа build
COPY --from=build /app/publish .

# Запускаем приложение
ENTRYPOINT ["dotnet", "RoomBooking.dll"]