FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Admin/PrimeApps.Admin.csproj", "PrimeApps.Admin/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.Admin/PrimeApps.Admin.csproj"
COPY . .

WORKDIR "/src/PrimeApps.Admin"
RUN dotnet build "PrimeApps.Admin.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Admin.csproj" --no-restore --self-contained false -c Debug -o /app

FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
COPY --from=publish /app .

EXPOSE 80
EXPOSE 443

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_HTTPS_PORT=443
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="1q2w3e4r5t"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="aspnetapp.pfx"

# Trust Kubernetes CA certificate
RUN mkdir -p /usr/local/share/ca-certificates/ && cp ca.crt /usr/local/share/ca-certificates/kubernetes_ca.crt
RUN chmod 777 /usr/local/share/ca-certificates/kubernetes_ca.crt
RUN update-ca-certificates --fresh

# Install PostgreSQL Client
RUN mkdir -p /usr/share/man/man1 && mkdir -p /usr/share/man/man7
RUN apt-get update && apt-get install -y --no-install-recommends postgresql-client-9.6
RUN psql --version

FROM base AS final
WORKDIR /app

ENTRYPOINT ["dotnet","PrimeApps.Admin.dll"]