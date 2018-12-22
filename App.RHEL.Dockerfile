FROM microsoft/dotnet:2.1-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT Docker
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

FROM microsoft/dotnet:2.1-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.App/PrimeApps.App.csproj", "PrimeApps.App/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]

RUN dotnet restore "PrimeApps.App/PrimeApps.App.csproj"
COPY . .
WORKDIR "/src/PrimeApps.App"
RUN dotnet build "PrimeApps.App.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.App.csproj" --no-restore -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# Install Visual Studio Remote Debugger
RUN apt-get update && apt-get install -y --no-install-recommends unzip
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/vsdbg  

ENTRYPOINT ["dotnet","PrimeApps.App.dll"]