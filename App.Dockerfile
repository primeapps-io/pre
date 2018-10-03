FROM microsoft/dotnet:2.1.5-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="pWd"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/tls.pfx"

FROM microsoft/dotnet:2.1.403-sdk-alpine AS build
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
ENTRYPOINT libressl pkcs12 -inkey /domain-cert/tls.key -in /domain-cert/tls.crt -export -out /app/tls.pfx -passout pass:pWd && dotnet PrimeApps.App.dll
