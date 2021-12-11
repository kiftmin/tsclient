#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

## tzdata for timzone
#RUN apt-get update -y
#RUN apt-get install -y tzdata
 #
## timezone env with default
#ENV TZ Africa/Johannesburg

#FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
#WORKDIR /src
#COPY ["BranSystems.Container.RFIDReader/BranSystems.Container.RFIDReader.csproj", "BranSystems.Container.RFIDReader/"]
#COPY ["BranSystems.RFIDReader.MQTT/BranSystems.MQTT.Device.RFIDReader.csproj", "BranSystems.RFIDReader.MQTT/"]
#COPY ["BranSystems.MQTT/BranSystems.MQTT.csproj", "BranSystems.MQTT/"]
#RUN dotnet restore "BranSystems.Container.RFIDReader/BranSystems.Container.RFIDReader.csproj"
#COPY . .
#WORKDIR "/src/BranSystems.Container.RFIDReader"
#RUN dotnet build "BranSystems.Container.RFIDReader.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "BranSystems.Container.RFIDReader.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
#COPY --from=publish /app/publish .
COPY bin/Release/net5.0/publish/ .
COPY mxio-library-x86-x64-2.12.sh /tmp
RUN chmod +x /tmp/mxio-library-x86-x64-2.12.sh
RUN sh /tmp/mxio-library-x86-x64-2.12.sh
RUN apt-get update && apt-get install -y iputils-ping
CMD bash
ENTRYPOINT ["dotnet", "BranSystems.Container.IO.dll"]