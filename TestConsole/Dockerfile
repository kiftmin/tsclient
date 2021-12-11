#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
#FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
#FROM mcr.microsoft.com/dotnet/sdk:5.0 AS base
WORKDIR /app

## tzdata for timzone
#RUN apt-get update -y
#RUN apt-get install -y tzdata
 #
## timezone env with default
#ENV TZ Africa/Johannesburg

#FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
#WORKDIR /src
#COPY ["TestConsole/TestConsole.csproj", "TestConsole/"]
#COPY ["BranSystems.RFIDReader.MQTT/BranSystems.MQTT.Device.RFIDReader.csproj", "BranSystems.RFIDReader.MQTT/"]
#COPY ["BranSystems.MQTT/BranSystems.MQTT.csproj", "BranSystems.MQTT/"]
#RUN dotnet restore "TestConsole/TestConsole.csproj"
#COPY . .
#WORKDIR "/src/TestConsole"
#RUN dotnet build "TestConsole.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "TestConsole.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
#COPY --from=publish /app/publish .
#COPY bin/Release/net5.0/publish/ App/
COPY bin/Release/net5.0/publish/ .
ENTRYPOINT ["dotnet", "TestConsole.dll"]
#ENTRYPOINT "/app/App/TestConsole.exe"
#ENTRYPOINT "/app/TestConsole.exe"