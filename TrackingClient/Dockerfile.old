#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TrackingClient/TrackingClient.csproj", "TrackingClient/"]
COPY ["BranSystems.Container.RFIDReader/BranSystems.Container.RFIDReader.csproj", "BranSystems.Container.RFIDReader/"]
COPY ["BranSystems.RFIDReader.MQTT/BranSystems.MQTT.Device.RFIDReader.csproj", "BranSystems.RFIDReader.MQTT/"]
COPY ["BranSystems.MQTT/BranSystems.MQTT.csproj", "BranSystems.MQTT/"]
COPY ["BranSystems.Container.IO/BranSystems.Container.IO.csproj", "BranSystems.Container.IO/"]
COPY ["BranSystems.MQTT.Device.IOController/BranSystems.MQTT.Device.IOController.csproj", "BranSystems.MQTT.Device.IOController/"]
RUN dotnet restore "TrackingClient/TrackingClient.csproj"
COPY . .
WORKDIR "/src/TrackingClient"
RUN dotnet build "TrackingClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrackingClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrackingClient.dll"]